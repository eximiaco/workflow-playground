using Microsoft.EntityFrameworkCore;
using WorkflowPlayground.Infra;
using WorkflowPlayground.Infra.Services;

namespace WorkflowPlayground.Domain;

public class OrderProcessingBackgroundService(IServiceProvider sp, ILogger<OrderProcessingBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = sp.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<PlaygroundDbContext>();
        var stockService = scope.ServiceProvider.GetRequiredService<StockService>();
        var paymentService = scope.ServiceProvider.GetRequiredService<PaymentService>();
        var shippingService = scope.ServiceProvider.GetRequiredService<ShippingService>();

        while (!stoppingToken.IsCancellationRequested)
        {
            var pendingOrders = await dbContext
                .Orders
                .Where(od => od.Status == OrderStatus.Pending)
                .ToListAsync(cancellationToken: stoppingToken);

            foreach (var order in pendingOrders)
            {
                logger.LogInformation("Starting background order processing");

                var reservationId = await stockService.ReserveAsync(order);
                order.SetAsStockReserved(reservationId);

                var paymentId = await paymentService.MakePaymentAsync(order);
                order.SetAsPaymentApproved(paymentId);

                await stockService.ConfirmReservationAsync(reservationId);

                var shippingId = await shippingService.ShipAsync(order);
                order.SetAsShipped(shippingId);
                
                order.SetAsProcessed();

                await dbContext.SaveChangesAsync(stoppingToken);

                logger.LogInformation("Finished background order processing");
            }

            await Task.Delay(5000, stoppingToken);
        }
    }
}