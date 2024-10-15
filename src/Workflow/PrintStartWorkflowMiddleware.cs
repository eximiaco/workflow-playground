using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace WorkflowPlayground.Workflow
{
    public class PrintStartWorkflowMiddleware(ILogger<PrintStartWorkflowMiddleware> logger) : IWorkflowMiddleware
    {
        public WorkflowMiddlewarePhase Phase => WorkflowMiddlewarePhase.PreWorkflow;

        public Task HandleAsync(WorkflowInstance workflow, WorkflowDelegate next)
        {
            logger.LogInformation("----- Starting Workflow {WorkflowName} -----", workflow.WorkflowDefinitionId);
            return next();
        }
    }

    public class PrintEndWorkflowMiddleware(ILogger<PrintEndWorkflowMiddleware> logger) : IWorkflowMiddleware
    {
        public WorkflowMiddlewarePhase Phase => WorkflowMiddlewarePhase.PostWorkflow;

        public Task HandleAsync(WorkflowInstance workflow, WorkflowDelegate next)
        {
            logger.LogInformation("----- Workflow {WorkflowName} Finished -----", workflow.WorkflowDefinitionId);
            return next();
        }
    }

}
