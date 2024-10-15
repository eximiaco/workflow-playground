using Microsoft.EntityFrameworkCore;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowPlayground.Infra;
using WorkflowPlayground.Infra.Services;

namespace WorkflowPlayground.Workflow.Steps;

public class ProcessPaymentStep(
    ILogger<ProcessPaymentStep> logger,
    PlaygroundDbContext dbContext,
    PaymentService paymentService) : StepBodyAsync
{
    public string OrderId { get; set; } = string.Empty;

    public async override Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        try
        {
            
            var order = await dbContext
                .Orders
                .FirstAsync(or => or.Id == OrderId, context.CancellationToken);

            var paymentId = await paymentService.MakePaymentAsync(order);

            order.SetAsPaymentApproved(paymentId);

            await dbContext.SaveChangesAsync(context.CancellationToken);

            return ExecutionResult.Next();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error ocurred processing payment");
            
            var timeToSleep = TimeSpan.FromSeconds(++context.ExecutionPointer.RetryCount * 5);

            return ExecutionResult.Sleep(timeToSleep, persistenceData: null);
        }
    }
}