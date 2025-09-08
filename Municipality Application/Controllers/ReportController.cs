using Microsoft.AspNetCore.Mvc;

namespace Municipality_Application.Controllers
{
    public class ReportController : Controller
    {
        public IActionResult ReportIssue()
        {
            return View();
        }

        public IActionResult ServiceRequestStatus()
        {
            return View();
        }
    }
}
