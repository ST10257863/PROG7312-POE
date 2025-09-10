using Microsoft.AspNetCore.Mvc;
using Municipality_Application.Interfaces;
using Municipality_Application.Models;

namespace Municipality_Application.Controllers
{
    /// <summary>
    /// Controller for handling report-related actions such as submitting issues and viewing confirmations.
    /// </summary>
    public class ReportController : Controller
    {
        private readonly IReportRepository _reportRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportController"/> class.
        /// </summary>
        /// <param name="reportRepository">The report repository instance.</param>
        public ReportController(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        /// <summary>
        /// Displays the form for reporting a new issue.
        /// </summary>
        /// <returns>The report issue view.</returns>
        public IActionResult ReportIssue()
        {
            return View();
        }

        /// <summary>
        /// Displays the service request status view.
        /// </summary>
        /// <returns>The service request status view.</returns>
        public IActionResult ServiceRequestStatus()
        {
            return View();
        }

        /// <summary>
        /// Handles the submission of a new issue report, including file attachments.
        /// </summary>
        /// <param name="report">The report data submitted by the user.</param>
        /// <param name="files">The list of files attached to the report.</param>
        /// <returns>
        /// Redirects to the confirmation page if successful; otherwise, redisplays the form with validation errors.
        /// </returns>
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

        /// <summary>
        /// Displays the confirmation page for a submitted report.
        /// </summary>
        /// <param name="id">The unique identifier of the report.</param>
        /// <returns>
        /// The confirmation view with report details if found; otherwise, a 404 Not Found result.
        /// </returns>
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
