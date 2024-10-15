using System.Text;
using WorkflowPlayground.Domain;

namespace WorkflowPlayground.Infra.Services;

public class ShippingService(HttpClient httpClient)
{
    private sealed record ShippingResponse(string ShippingId);

    public async Task<string> ShipAsync(Order order)
    {
        var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
        content.Headers.Add("idempotencyKey", order.Id);
        var response = await httpClient.PostWithDelayAsync("shipping", content);

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadFromJsonAsync<ShippingResponse>();
        return responseContent!.ShippingId;
    }

    public async Task CancelShippingAsync(string? shippingId)
    {
        var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
        var response = await httpClient.PostWithDelayAsync($"shipping/{shippingId}/cancel", content);

        response.EnsureSuccessStatusCode();
    }
}