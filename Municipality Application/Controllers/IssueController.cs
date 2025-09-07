using Microsoft.AspNetCore.Mvc;

namespace Municipality_Application.Controllers
{
    public class IssueController : Controller
    {
        public IActionResult ReportIssue()
        {
            return View();
        }
    }
}
