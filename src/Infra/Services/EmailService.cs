using WorkflowPlayground.Domain;

namespace WorkflowPlayground.Infra.Services;

public class EmailService
{
    public async Task SendEmailAsync(Order order)
    {
        await Task.Delay(1000);
    }
}