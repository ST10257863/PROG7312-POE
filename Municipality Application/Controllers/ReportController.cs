using Microsoft.AspNetCore.Mvc;
using Municipality_Application.Interfaces;
using Municipality_Application.Models;

namespace Municipality_Application.Controllers
{
    public class ReportController : Controller
    {
        private readonly IReportRepository _reportRepository;

        public ReportController(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }


        public IActionResult ReportIssue()
        {
            return View();
        }

        public IActionResult ServiceRequestStatus()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ReportIssue(Report report, List<IFormFile> files)
        {
            #region Debugging
            Console.WriteLine($"Files received: {files?.Count ?? 0}");
            if (files != null)
            {
                foreach (var file in files)
                {
                    Console.WriteLine($"File: {file.FileName}, Size: {file.Length}");
                }
            }
            #endregion
            if (!ModelState.IsValid)
            {
                return View(report);
            }

            try
            {
                var savedReport = await _reportRepository.AddReportAsync(report, files);
                return RedirectToAction("Confirmation", new { id = savedReport.Id });
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while saving the report. Please try again.");
                return View(report);
            }
        }

        public async Task<IActionResult> Confirmation(Guid id)
        {
            var report = await _reportRepository.GetReportByIdAsync(id);
            if (report == null)
            {
                return NotFound();
            }
            return View(report);
        }
    }
}
