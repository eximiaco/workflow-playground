using Microsoft.EntityFrameworkCore;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowPlayground.Infra;
using WorkflowPlayground.Infra.Services;

namespace WorkflowPlayground.Workflow.Steps;

public class ScheduleShippingStep(
    PlaygroundDbContext dbContext,
    ShippingService shippingService) : StepBodyAsync
{
    public string OrderId { get; set; } = string.Empty;

    public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        var order = await dbContext
            .Orders
            .FirstAsync(or => or.Id == OrderId);

        var shippingId = await shippingService.ShipAsync(order);

        order.SetAsShipped(shippingId);

        await dbContext.SaveChangesAsync(context.CancellationToken);

        return ExecutionResult.Next();
    }
}
