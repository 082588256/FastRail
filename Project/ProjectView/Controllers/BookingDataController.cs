using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Text;

namespace ProjectView.Controllers
{
    public class BookingDataController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public BookingDataController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetTrips()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5014";
                
                Console.WriteLine($"Calling API: {apiBaseUrl}/api/trips/all");
                
                var response = await client.GetAsync($"{apiBaseUrl}/api/trips/all");
                
                Console.WriteLine($"API Response Status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Response Content: {responseContent}");
                    // Return the response content directly without deserializing
                    return Content(responseContent, "application/json; charset=utf-8");
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Error Content: {errorContent}");
                return Json(new { success = false, message = $"Failed to fetch trips. Status: {response.StatusCode}" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetTrips: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSeats(int tripId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5014";
                
                var response = await client.GetAsync($"{apiBaseUrl}/api/trips/{tripId}/seats");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var seats = JsonSerializer.Deserialize<List<dynamic>>(content);
                    return Json(new { success = true, data = seats });
                }
                
                return Json(new { success = false, message = "Failed to fetch seats" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
} 