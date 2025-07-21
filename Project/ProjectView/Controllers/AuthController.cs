using Microsoft.AspNetCore.Mvc;

namespace ProjectView.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult StaffLogin()
        {
            return View();
        }
    }
} 