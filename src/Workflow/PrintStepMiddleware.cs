using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace WorkflowPlayground.Workflow;

public class PrintStepMiddleware(ILogger<PrintStepMiddleware> log) : IWorkflowStepMiddleware
{
    public async Task<ExecutionResult> HandleAsync(
        IStepExecutionContext context,
        IStepBody body,
        WorkflowStepDelegate next)
    {
        if (string.IsNullOrEmpty(context.Step.Name))
        {
            return await next();
        }
        
        log.LogInformation("{Id} - Starting {StepName}", context.Step.Id - 1, context.Step.Name);
        var executionResult = await next();
        log.LogInformation("{StepName} Finished", context.Step.Name);

        return executionResult;
    }
}