using System.Text;
using WorkflowPlayground.Domain;

namespace WorkflowPlayground.Infra.Services;

public class PaymentService(HttpClient httpClient)
{
    private sealed record PaymentResponse(string PaymentId);

    public async Task<string> MakePaymentAsync(Order order)
    {
        var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
        content.Headers.Add("idempotencyKey", order.Id);
        var response = await httpClient.PostWithDelayAsync("payments", content);

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadFromJsonAsync<PaymentResponse>();
        return responseContent!.PaymentId;
    }

    public async Task RefundPaymentAsync(string? paymentId)
    {
        var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
        var response = await httpClient.PostWithDelayAsync($"payments/{paymentId}/refund", content);

        response.EnsureSuccessStatusCode();

        await response.Content.ReadFromJsonAsync<PaymentResponse>();
    }
}