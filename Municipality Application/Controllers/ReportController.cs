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
            // Log all model values for debugging
            System.Diagnostics.Debug.WriteLine("---- ReportCreateViewModel Submission ----");
            System.Diagnostics.Debug.WriteLine($"Description: {model.Description}");
            System.Diagnostics.Debug.WriteLine($"CategoryId: {model.CategoryId}");
            if (model.Address != null)
            {
                System.Diagnostics.Debug.WriteLine($"Address.Street: {model.Address.Street}");
                System.Diagnostics.Debug.WriteLine($"Address.Suburb: {model.Address.Suburb}");
                System.Diagnostics.Debug.WriteLine($"Address.City: {model.Address.City}");
                System.Diagnostics.Debug.WriteLine($"Address.Province: {model.Address.Province}");
                System.Diagnostics.Debug.WriteLine($"Address.PostalCode: {model.Address.PostalCode}");
                System.Diagnostics.Debug.WriteLine($"Address.Country: {model.Address.Country}");
                System.Diagnostics.Debug.WriteLine($"Address.Latitude: {model.Address.Latitude}");
                System.Diagnostics.Debug.WriteLine($"Address.Longitude: {model.Address.Longitude}");
                System.Diagnostics.Debug.WriteLine($"Address.FormattedAddress: {model.Address.FormattedAddress}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Address: null");
            }
            System.Diagnostics.Debug.WriteLine($"PhoneNumber: {model.PhoneNumber}");
            System.Diagnostics.Debug.WriteLine($"Email: {model.Email}");
            System.Diagnostics.Debug.WriteLine($"Files.Count: {model.Files?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine("------------------------------------------");
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
            var viewModel = ReportMapper.ToConfirmationViewModel(report);
            return View("Confirmation", viewModel);
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
