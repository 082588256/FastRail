using Microsoft.AspNetCore.Mvc;

namespace ProjectView.Controllers
{
    [Route("/Admin/RouteManagement")]
    public class RouteManagementController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Views/Admin/RouteManagement.cshtml");
        }
    }
}
