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
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            var model = new ReportCreateViewModel
            {
                Categories = categories
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ReportCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _categoryRepository.GetAllCategoriesAsync();
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
                model.Categories = await _categoryRepository.GetAllCategoriesAsync();
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
            model.Statuses = ReportMapper.GetStatusSelectList();
            model.Categories = await _categoryRepository.GetAllCategoriesAsync();

            var reports = await _reportService.ListReportsFilteredAsync(
                model.SearchReportId,
                model.SearchTitle,
                model.SearchArea,
                model.StartDate,
                model.EndDate,
                model.CategoryId,
                model.Status);

            model.Results = reports.Select(ReportMapper.ToViewModel).ToList();
            return View(model);
        }

        public async Task<IActionResult> ServiceRequestStatusInformation(Guid id)
        {
            var report = await _reportService.GetReportDetailsAsync(id);
            if (report == null)
            {
                return NotFound();
            }
            var viewModel = ReportMapper.ToServiceRequestStatusViewModel(report);
            return View("ServiceRequestStatusInformation", viewModel);
        }
    }
}
