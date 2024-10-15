using Microsoft.EntityFrameworkCore;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowPlayground.Infra;
using WorkflowPlayground.Infra.Services;

namespace WorkflowPlayground.Workflow.Steps;

public class ConfirmStockStep(
    PlaygroundDbContext dbContext,
    StockService stockService) : StepBodyAsync
{
    public string OrderId { get; set; } = string.Empty;

    public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        var order = await dbContext
            .Orders
            .FirstAsync(or => or.Id == OrderId);

        await stockService.ConfirmReservationAsync(order.StockReservationId!);

        return ExecutionResult.Next();
    }
}