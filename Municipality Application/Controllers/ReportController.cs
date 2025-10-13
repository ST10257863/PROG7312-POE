using Microsoft.AspNetCore.Mvc;
using Municipality_Application.Interfaces;
using Municipality_Application.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Municipality_Application.Controllers
{
    /// <summary>
    /// Controller for handling report-related actions such as submitting issues and viewing confirmations.
    /// </summary>
    public class ReportController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IReportService _reportService;

        public string GoogleMapsApiKey { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportController"/> class.
        /// </summary>
        /// <param name="reportService">The report service instance.</param>
        /// <param name="config">The configuration instance.</param>
        public ReportController(IReportService reportService, IConfiguration config)
        {
            _reportService = reportService;
            _config = config;
        }

        /// <summary>
        /// Displays the form for reporting a new issue.
        /// </summary>
        /// <returns>The report issue view.</returns>
        public IActionResult Index()
        {
            var googleMapsKey = _config["ApiKeys:GoogleMaps"];
            ViewBag.GoogleMapsApiKey = string.IsNullOrWhiteSpace(googleMapsKey) ? null : googleMapsKey;
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
        public async Task<IActionResult> Create(Report report, List<IFormFile> files)
        {
            if (!ModelState.IsValid)
            {
                return View(report);
            }

            try
            {
                var savedReport = await _reportService.SubmitReportAsync(report, files);
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
            var report = await _reportService.GetReportDetailsAsync(id);
            if (report == null)
            {
                return NotFound();
            }
            return View("Confirmation", report);
        }
    }
}
