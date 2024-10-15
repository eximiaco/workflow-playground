using Microsoft.EntityFrameworkCore;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowPlayground.Infra;
using WorkflowPlayground.Infra.Services;

namespace WorkflowPlayground.Workflow.Steps;

public class UndoProcessingStep(
    PlaygroundDbContext dbContext,
    ShippingService shippingService,
    StockService stockService,
    PaymentService paymentService,
    ILogger<UndoProcessingStep> logger) : StepBodyAsync
{
    public string OrderId { get; set; } = string.Empty;

    public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        var order = await dbContext
            .Orders
            .FirstAsync(o => o.Id == OrderId, context.CancellationToken);

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

        order.Cancel();

        await dbContext.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation("Order {OrderId} was canceled!", order.Id);

        return ExecutionResult.Next();
    }
}