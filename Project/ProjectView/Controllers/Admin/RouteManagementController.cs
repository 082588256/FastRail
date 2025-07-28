using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Project.Models;
using ProjectView.Filter;
using ProjectView.Models.Admin.Route;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

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
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseURL"];
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var token = HttpContext.Session.GetString("JWT");

            var baseUrl= _configuration["ApiSettings:BaseURL"];
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync($"{baseUrl}/api/Station");
            if (!response.IsSuccessStatusCode)
            {
                return (BadRequest());
            }
            var content= await response.Content.ReadAsStringAsync();
            var stations = JsonConvert.DeserializeObject<List<Station>>(content);

            var stationItems = stations.Select(s => new SelectListItem
            {
                Value = s.StationId.ToString(),
                Text = s.StationName
            }).ToList();
            // Lưu vào ViewBag
            ViewBag.Stations = stationItems;
            ViewBag.StationList = stationItems;
            

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

        [HttpPost]
        public async Task<IActionResult> Create(CreateRouteRequest dto)
        {
            Console.WriteLine(">> POST Create CALLED"); // 
            if (!ModelState.IsValid)
            {
                // Lấy danh sách station để hiển thị lại dropdown
                await LoadStationListAsync(); // Giả sử bạn có method này
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                ViewBag.Errors = errors;
                
            }

            var token = HttpContext.Session.GetString("JWT");
            var baseUrl = _configuration["ApiSettings:BaseURL"];
            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // 🔧 Bọc object lại đúng format { routeDTO: dto }
            
            var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{baseUrl}/api/Route", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Route created successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                await LoadStationListAsync(); // để hiển thị lại form nếu failed
                ViewBag.Error = "Đã có lỗi xảy ra: " + errorContent;
                return View(dto);
            }
        }


        private async Task LoadStationListAsync()
        {
            var token = HttpContext.Session.GetString("JWT");
            var baseUrl = _configuration["ApiSettings:BaseURL"];
            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync($"{baseUrl}/api/Station");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var stations = JsonConvert.DeserializeObject<List<Station>>(content);
                ViewBag.StationList = stations.Select(s => new SelectListItem
                {
                    Value = s.StationId.ToString(),
                    Text = s.StationName
                }).ToList();
            }
            else
            {
                ViewBag.StationList = new List<SelectListItem>();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var token = HttpContext.Session.GetString("JWT");
            var baseUrl = _configuration["ApiSettings:BaseURL"];
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{baseUrl}/api/Route/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound(); // hoặc xử lý khác
            }

            var json = await response.Content.ReadAsStringAsync();
            var route = JsonConvert.DeserializeObject<UpdateRouteRequest>(json);

            await LoadStationListAsync(); // nếu cần select dropdown
            
            return View(route);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var token = HttpContext.Session.GetString("JWT");
            var baseUrl = _configuration["ApiSettings:BaseURL"];
            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.DeleteAsync($"{baseUrl}/api/Route/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Tuyến đã được xóa thành công.";
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                TempData["Error22"] = "Xóa thất bại: " + errorContent;
            }

            return RedirectToAction("Index");
        }

    }
}
