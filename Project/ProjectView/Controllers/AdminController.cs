using Microsoft.AspNetCore.Mvc;
using ProjectView.Filter;

namespace ProjectView.Controllers
{
    
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            var token = HttpContext.Session.GetString("JWT");

            if (string.IsNullOrEmpty(token))
            {
                
                return View("~/Views/Authen/Login/Index.cshtml");
            }


            return View("Dashboard");
        }
    }
}
