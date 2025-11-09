using Microsoft.AspNetCore.Mvc;
using Municipality_Application.Interfaces;
using Municipality_Application.Models;
using Municipality_Application.Interfaces.Service;
using Municipality_Application.Mappers;
using Municipality_Application.ViewModels;

namespace Municipality_Application.Controllers
{
    public class ReportController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IReportService _reportService;
        private readonly ICategoryRepository _categoryRepository;

        public ReportController(IReportService reportService, IConfiguration config, ICategoryRepository categoryRepository)
        {
            _reportService = reportService;
            _config = config;
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            var googleMapsKey = _config["ApiKeys:GoogleMaps"];
            ViewBag.GoogleMapsApiKey = string.IsNullOrWhiteSpace(googleMapsKey) ? null : googleMapsKey;

            var categories = await _categoryRepository.GetAllCategoriesAsync();
            ViewBag.Categories = categories;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Report report, List<IFormFile> files)
        {
            if (!double.TryParse(Request.Form["Latitude"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var lat))
                report.Latitude = null;
            else
                report.Latitude = lat;
            if (!double.TryParse(Request.Form["Longitude"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var lng))
                report.Longitude = null;
            else
                report.Longitude = lng;
            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    foreach (var error in errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"{key}: {error.ErrorMessage}");
                    }
                }
                ViewBag.Categories = await _categoryRepository.GetAllCategoriesAsync();
                return View("Index", report);
            }

            try
            {
                var savedReport = await _reportService.SubmitReportAsync(report, files);
                return RedirectToAction("Confirmation", new { id = savedReport.Id });
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while saving the report. Please try again.");
                ViewBag.Categories = await _categoryRepository.GetAllCategoriesAsync();
                return View("Index", report);
            }
        }

        public async Task<IActionResult> Confirmation(Guid id)
        {
            var report = await _reportService.GetReportDetailsAsync(id);
            if (report == null)
            {
                return NotFound();
            }
            return View("Confirmation", report);
        }

        public async Task<IActionResult> ServiceRequestStatus(ServiceRequestStatusPageViewModel model)
        {
            var reports = await _reportService.ListReportsFilteredAsync(
                model.SearchTitle,
                model.SearchArea,
                model.StartDate,
                model.EndDate);

            model.Results = reports.Select(ReportMapper.ToViewModel).ToList();
            return View(model);
        }
    }
}
