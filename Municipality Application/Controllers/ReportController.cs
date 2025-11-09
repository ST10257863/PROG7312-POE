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
        public async Task<IActionResult> Create(ReportCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryRepository.GetAllCategoriesAsync();
                return View("Index", model);
            }

            // Map ViewModel to Domain Model using a mapper
            var report = ReportMapper.ToDomainModel(model);

            try
            {
                // Service handles business logic and attachments
                var savedReport = await _reportService.SubmitReportAsync(report, model.Files ?? new List<IFormFile>());
                return RedirectToAction("Confirmation", new { id = savedReport.Id });
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while saving the report. Please try again.");
                ViewBag.Categories = await _categoryRepository.GetAllCategoriesAsync();
                return View("Index", model);
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
