using Microsoft.EntityFrameworkCore;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowPlayground.Infra;
using WorkflowPlayground.Infra.Services;

namespace WorkflowPlayground.Workflow.Steps;

public class NotifyCostumerStep(
    PlaygroundDbContext dbContext,
    EmailService emailService) : StepBodyAsync
{
    public string OrderId { get; set; } = string.Empty;

    public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        var order = await dbContext
            .Orders
            .FirstAsync(or => or.Id == OrderId);

        await emailService.SendEmailAsync(order);

        return ExecutionResult.Next();
    }
}