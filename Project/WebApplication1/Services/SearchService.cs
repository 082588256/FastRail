using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebApplication1.Models; 

public interface ISearchService
{
    Task<BookingViewModel?> GetTicketByBookingCodeAsync(string bookingCode);
}
public class SearchService : ISearchService
{
    private readonly HttpClient _httpClient;

    public SearchService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BookingViewModel?> GetTicketByBookingCodeAsync(string bookingCode)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(new { BookingCode = bookingCode }),
            Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("Booking/guest-lookup", content);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<BookingViewModel>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result?.Data;
    }

}
