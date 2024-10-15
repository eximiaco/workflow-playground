using System.Text;
using WorkflowPlayground.Domain;

namespace WorkflowPlayground.Infra.Services;

public class StockService(HttpClient httpClient)
{
    private sealed record StockReservationResponse(string ReservationId);

    public async Task<string> ReserveAsync(Order order)
    {
        var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
        content.Headers.Add("idempotencyKey", order.Id);
        var response = await httpClient.PostWithDelayAsync("stock-reservation", content);

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadFromJsonAsync<StockReservationResponse>();
        return responseContent!.ReservationId;
    }

    public async Task ConfirmReservationAsync(string stockReservationId)
    {
        var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
        var response = await httpClient.PostWithDelayAsync($"stock-confirmation/{stockReservationId}", content);

        response.EnsureSuccessStatusCode();
    }

    public async Task UndoStockReservationAsync(string stockReservationId)
    {
        var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
        var response = await httpClient.PostWithDelayAsync($"stock-reservation/{stockReservationId}/cancel", content);

        response.EnsureSuccessStatusCode();
    }
}