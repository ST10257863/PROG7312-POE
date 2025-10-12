using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Municipality_Application.Controllers
{
    public class EventsAndAnnouncementsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
