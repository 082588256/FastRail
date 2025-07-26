using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectView.Filter;
using ProjectView.Models.Admin.Dashboard;

namespace ProjectView.Controllers
{
    [AdminOnly]
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly IConfiguration _configuration;

        public AdminController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory;
            _configuration = configuration;
            
        }
        public async Task<IActionResult> Index()
        {

            var token = HttpContext.Session.GetString("JWT");
            var baseUrl = _configuration["ApiSettings:BaseURL"];
            if (string.IsNullOrEmpty(token))
            {
                return View("~/Views/Authen/Login.cshtml");
            }

            var client= _httpClient.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response= await client.GetAsync($"{baseUrl}/api/admin/Dashboard");

            if(response.StatusCode== System.Net.HttpStatusCode.Forbidden)
            {
                return Forbid();
            }

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Không thể load được dữ liệu";
                return View("Dashboard");
            }
            
            var content= await response.Content.ReadAsStringAsync();
            try
            {
                var stats = JsonConvert.DeserializeObject<AdminDashBoardViewModel>(content);
                return View("Dashboard", stats);
            }
            catch (Exception ex)
            {
                ViewBag["error"] = ex.Message;
                return View("Dashboard");
            }
            
        }

        public IActionResult Revenue()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> getTripData()
        {
            var token = HttpContext.Session.GetString("JWT");
            var baseUrl = _configuration["ApiSettings:baseUrl"];
            var client = _httpClient.CreateClient();
            client.DefaultRequestHeaders.Authorization= new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{baseUrl}/api/Admin/chart-trip");
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode);
            }

            var content= await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpGet]
        public async Task<IActionResult> getSeatData()
        {
            var token = HttpContext.Session.GetString("JWT");
            var baseUrl = _configuration["ApiSettings:baseUrl"];
            var client = _httpClient.CreateClient();
            client.DefaultRequestHeaders.Authorization= new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync($"{baseUrl}/api/Admin/chart-seat");
            if(!response.IsSuccessStatusCode)
            {
                return StatusCode((int)(response.StatusCode));
            }
            var content= await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpGet]
        public async Task<IActionResult> getTopTrips()
        {
            var token = HttpContext.Session.GetString("JWT");
            var baseUrl = _configuration["ApiSettings:baseUrl"];
            var client = _httpClient.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync($"{baseUrl}/api/Admin/chart-top-trip");
            if(!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode);
            }
            var content= await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpGet]
        public async Task<IActionResult> getChartRevenue()
        {
            var token = HttpContext.Session.GetString("JWT");
            var baseUrl = _configuration["ApiSettings:baseUrl"];
            var client = _httpClient.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync($"{baseUrl}/api/Admin/chart-revenue");
            if(!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode);
            }
            var content= await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");

        }

        [HttpGet]
        public async Task<IActionResult> getRevenueByBookType()
        {
            var token = HttpContext.Session.GetString("JWT");
            var baseUrl = _configuration["ApiSettings:baseUrl"];
            var client = _httpClient.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync($"{baseUrl}/api/Admin/chart-revenue-user");
            if(!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode);
            }
            var content= await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpGet]
        public async Task<IActionResult> getRevenueBySeatType()
        {
            var token = HttpContext.Session.GetString("JWT");
            var baseUrl = _configuration["ApiSettings:baseUrl"];
            var client = _httpClient.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync($"{baseUrl}/api/Admin/chart-revenue-seattype");
            if(!response.IsSuccessStatusCode)
            {
                return StatusCode((int)(response.StatusCode));
            }
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
        public class BookingTypeRevenueDto
        {
            public string BookingType { get; set; } = default!;
            public decimal TotalRevenue { get; set; }
        }
    }
}
