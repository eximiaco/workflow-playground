using Microsoft.EntityFrameworkCore;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowPlayground.Infra;

namespace WorkflowPlayground.Workflow.Steps;

public class FinishOrderStep(PlaygroundDbContext dbContext) : StepBodyAsync
{
    public string OrderId { get; set; } = string.Empty;
    
    public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        var order = await dbContext
            .Orders
            .FirstAsync(or => or.Id == OrderId);

        order.SetAsProcessed();

        await dbContext.SaveChangesAsync(context.CancellationToken);

        return ExecutionResult.Next();
    }
}