using Microsoft.AspNetCore.Mvc;

namespace ProjectView.Controllers
{
    public class FareController : Controller 
    {
        public IActionResult Index()
        {
            return View(); // Trả về Views/Fare/Index.cshtml
        }
    }
}