using System.Text.Json;
using ProjectView.Models;

namespace ProjectView.Services
{
    public interface ITripService
    {
        Task<List<TripDetailViewModel>> SearchTripsAsync(TripSearchRequest request);
    }
    public class TripService : ITripService
    {
        private readonly HttpClient _httpClient;
        public TripService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<TripDetailViewModel>> SearchTripsAsync(TripSearchRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/Trips/search", request);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<List<TripDetailViewModel>>(json, options)
                   ?? new List<TripDetailViewModel>();
        }

    }

}
