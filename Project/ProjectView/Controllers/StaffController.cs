using Microsoft.AspNetCore.Mvc;

namespace ProjectView.Controllers
{
    public class StaffController : Controller
    {
        public IActionResult QRScanner()
        {
            return View();
        }
    }
} 