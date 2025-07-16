using Microsoft.AspNetCore.Mvc;

namespace ProjectView.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View("Dashboard");
        }
    }
}
