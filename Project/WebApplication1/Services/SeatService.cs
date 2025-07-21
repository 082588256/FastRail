using System.Text.Json;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface ISeatService
    {
        Task<List<SeatAvailabilityResponse>> GetAvailableSeatsAsync(int tripId, int fromStationId, int toStationId);
    }
    public class SeatService : ISeatService
    {
        private readonly HttpClient _httpClient;

        public SeatService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<SeatAvailabilityResponse>> GetAvailableSeatsAsync(int tripId, int fromStationId, int toStationId)
        {
            var url = $"api/Trips/{tripId}/seats?fromStationId={fromStationId}&toStationId={toStationId}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<SeatAvailabilityResponse>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<SeatAvailabilityResponse>();
        }

    }
    }
