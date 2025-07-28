using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;
using ProjectView.Models.Auth;

namespace ProjectView.Controllers
{
    public class AuthenController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AuthenController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(Models.Auth.LoginRequest model)
        {
            ViewBag.HideNavbar = true;
            var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/api/Auth/login";
            var client = _httpClientFactory.CreateClient();

            var response = await client.PostAsJsonAsync(apiUrl, model);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result != null && result.Success)
                {
                    HttpContext.Session.SetString("JWT", result.Token);
                    return RedirectToAction("Index", "Admin");
                }
            }

            ViewBag.Error = "Sai email hoặc mật khẩu.";
            return View("Login", model);
        }
    }
}

