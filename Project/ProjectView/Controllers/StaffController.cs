using Microsoft.AspNetCore.Mvc;

namespace ProjectView.Controllers
{
    public class StaffController : Controller
    {
        public IActionResult Management()
        {
            return View();
        }

        public IActionResult QRScanner()
        {
            return View();
        }

        public IActionResult CustomerList()
        {
            return View();
        }

        public IActionResult Test()
        {
            return View();
        }

        public IActionResult TestQR()
        {
            return View();
        }
    }
} 