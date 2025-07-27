using Microsoft.AspNetCore.Mvc;
using ProjectView.Filter;
using System.Net.Http;

namespace ProjectView.Controllers.Admin
{
    [AdminOnly]
    
    public class RouteManagementController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RouteManagementController> _logger;

        public RouteManagementController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<RouteManagementController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetAllRoutes()
        {
            var token = HttpContext.Session.GetString("JWT");
            var baseUrl = _configuration["ApiSettings:BaseURL"];
            
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync($"{baseUrl}/api/route");
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode);
            }
            var content= await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
    }
}
