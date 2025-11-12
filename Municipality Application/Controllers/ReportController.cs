using Microsoft.AspNetCore.Mvc;
using Municipality_Application.Interfaces;
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

            // --- Heap-based prioritization for urgent requests ---
            // This MinHeap structure supports O(log n) extraction of the most urgent unresolved requests.
            var urgentReports = await _reportService.GetTopUrgentReportsAsync(5);
            model.TopUrgentReports = urgentReports.Select(ReportMapper.ToViewModel).ToList();

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

        /// <summary>
        /// [BST] Returns all service requests sorted by CreatedDate using a Binary Search Tree (BST).
        /// Demonstrates O(log n) search and efficient in-order retrieval.
        /// </summary>
        /// <returns>Sorted list of service requests.</returns>
        [HttpGet]
        [Route("Reports/TreeView")]
        public async Task<IActionResult> TreeView()
        {
            var sortedReports = await _reportService.ListReportsSortedByDateAsync();
            var viewModels = sortedReports.Select(ReportMapper.ToServiceRequestStatusViewModel).ToList();
            return View("TreeView", viewModels);
        }

        /// <summary>
        /// [MinHeap] Returns the top N most urgent service requests using a MinHeap (priority queue).
        /// Demonstrates O(log n) extraction of the most urgent unresolved requests.
        /// </summary>
        /// <param name="count">Number of urgent requests to return (default 5).</param>
        /// <returns>List of urgent service requests.</returns>
        [HttpGet]
        [Route("Reports/PriorityQueue")]
        public async Task<IActionResult> PriorityQueue(int count = 5)
        {
            var urgentReports = await _reportService.GetTopUrgentReportsAsync(count);
            var viewModels = urgentReports.Select(ReportMapper.ToServiceRequestStatusViewModel).ToList();
            return View("PriorityQueue", viewModels);
        }

        /// <summary>
        /// [Graph] Visualizes related service requests using a graph traversal (BFS/DFS).
        /// Demonstrates adjacency list and traversal for finding related requests.
        /// </summary>
        /// <param name="id">The root request ID to visualize from.</param>
        /// <returns>List of related service requests discovered via graph traversal.</returns>
        [HttpGet]
        [Route("Reports/GraphView/{id}")]
        public async Task<IActionResult> GraphView(Guid id)
        {
            // Example: Use BFS to find related requests (e.g., by category, area, or other relation)
            var relatedReports = await _reportService.GetRelatedRequestsByGraphAsync(id);
            var viewModels = relatedReports.Select(ReportMapper.ToServiceRequestStatusViewModel).ToList();
            return View("GraphView", viewModels);
        }
    }
}
