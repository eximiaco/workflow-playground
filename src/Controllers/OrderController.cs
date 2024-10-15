using Microsoft.AspNetCore.Mvc;
using WorkflowCore.Interface;
using WorkflowPlayground.Domain;
using WorkflowPlayground.Infra;
using WorkflowPlayground.Infra.Services;
using WorkflowPlayground.Workflow;

namespace WorkflowPlayground.Controllers;

public record CreateOrderRequest(string ProductName, IEnumerable<OrderItemRequest> Items);
public record OrderItemRequest(int Amount, decimal Price);

[ApiController]
[Route("orders")]
public class OrderController : ControllerBase
{
    [HttpPost("synchronous")]
    public async Task<object> CreateOrder(
        [FromBody] CreateOrderRequest orderRequest,
        [FromServices] ILogger<OrderController> logger,
        [FromServices] PaymentService paymentService,
        [FromServices] ShippingService shippingService,
        [FromServices] StockService stockService,
        [FromServices] EmailService emailService,
        [FromServices] PlaygroundDbContext dbContext)
    {
        var ordeItems = orderRequest
                   .Items
                   .Select(item => new OrderItem(item.Amount, item.Price))
                   .ToList();

        var order = new Order(orderRequest.ProductName, ordeItems);

        try
        {
            logger.LogInformation("Starting synchronous order processing");

            var reservationId = await stockService.ReserveAsync(order);
            order.SetAsStockReserved(reservationId);

            var paymentId = await paymentService.MakePaymentAsync(order);
            order.SetAsPaymentApproved(paymentId);

            await stockService.ConfirmReservationAsync(reservationId);

            var shippingId = await shippingService.ShipAsync(order);
            order.SetAsShipped(shippingId);

            await emailService.SendEmailAsync(order);

            order.SetAsProcessed();

            dbContext.Add(order);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Finished synchronous order processing");

            return Ok(new
            {
                OrderId = order.Id
            });
        }
        catch (Exception ex)
        {
            if (order.WasStockReserved())
            {
                await stockService.UndoStockReservationAsync(order.StockReservationId!);
            }

            if (order.WasPaymentApproved())
            {
                await paymentService.RefundPaymentAsync(order.PaymentId);
            }

            if (order.WasShipped())
            {
                await shippingService.CancelShippingAsync(order.ShippingId);
            }

            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("background")]
    public async Task<object> CreateOrderBg(
        [FromBody] CreateOrderRequest orderRequest,
        [FromServices] PlaygroundDbContext dbContext)
    {
        var ordeItems = orderRequest
                   .Items
                   .Select(item => new OrderItem(item.Amount, item.Price))
                   .ToList();

        var order = new Order(orderRequest.ProductName, ordeItems);

        dbContext.Add(order);
        await dbContext.SaveChangesAsync();

        return Ok(new
        {
            OrderId = order.Id
        });
    }

    [HttpPost("workflow")]
    public async Task<object> CreateOrderWorkflowAsync(
        [FromBody] CreateOrderRequest orderRequest,
        [FromServices] PlaygroundDbContext dbContext,
        [FromServices] IWorkflowHost workflowHost)
    {
        var ordeItems = orderRequest
            .Items
            .Select(item => new OrderItem(item.Amount, item.Price))
            .ToList();

        var order = new Order(orderRequest.ProductName, ordeItems);

        dbContext.Add(order);
        await dbContext.SaveChangesAsync();

        var workflowId = await workflowHost.StartWorkflow("OrderProcessingWorkflow", new OrderWorflowData
        {
            OrderId = order.Id
        });

        return Ok(new
        {
            OrderId = order.Id,
            WorkflowId = workflowId
        });
    }
}