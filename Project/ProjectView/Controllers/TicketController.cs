using Microsoft.AspNetCore.Mvc;

namespace ProjectView.Controllers
{
    public class TicketController : Controller
    {
        public IActionResult Index()
        {
            return View(); // Trả về Views/Ticket/Index.cshtml
        }
    }
}
