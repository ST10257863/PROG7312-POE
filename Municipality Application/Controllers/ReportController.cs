using Microsoft.AspNetCore.Mvc;
using Municipality_Application.Interfaces;
using Municipality_Application.Interfaces.Service;
using Municipality_Application.Mappers;
using Municipality_Application.ViewModels;

namespace Municipality_Application.Controllers
{
    /// <summary>
    /// Handles report creation, status tracking, and advanced report visualizations for the municipality application.
    /// </summary>
    public class ReportController : Controller
    {
        #region Fields

        private readonly IReportService _reportService;
        private readonly ICategoryRepository _categoryRepository;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportController"/> class.
        /// </summary>
        /// <param name="reportService">The report service for managing reports.</param>
        /// <param name="config">The application configuration.</param>
        /// <param name="categoryRepository">The category repository for retrieving categories.</param>
        public ReportController(IReportService reportService, IConfiguration config, ICategoryRepository categoryRepository)
        {
            _reportService = reportService;
            _categoryRepository = categoryRepository;
        }

        #endregion

        #region Report Creation

        /// <summary>
        /// Displays the report creation page and shows the top urgent reports.
        /// </summary>
        /// <returns>The report creation view with categories and urgent reports.</returns>
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            var model = new ReportCreateViewModel
            {
                Categories = categories
            };

            var urgentReports = await _reportService.GetTopUrgentReportsAsync(5);
            model.TopUrgentReports = urgentReports.Select(ReportMapper.ToViewModel).ToList();

            return View(model);
        }

        /// <summary>
        /// Handles the submission of a new report.
        /// </summary>
        /// <param name="model">The report creation view model containing user input and files.</param>
        /// <returns>
        /// Redirects to the confirmation page if successful; otherwise, redisplays the form with validation errors.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> Create(ReportCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _categoryRepository.GetAllCategoriesAsync();
                return View("Index", model);
            }

            var report = ReportMapper.ToDomainModel(model);

            try
            {
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

        /// <summary>
        /// Displays the confirmation page for a successfully submitted report.
        /// </summary>
        /// <param name="id">The unique identifier of the submitted report.</param>
        /// <returns>The confirmation view if the report exists; otherwise, a 404 Not Found result.</returns>
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

        #endregion

        #region Service Request Status

        /// <summary>
        /// Displays the service request status page with filtering and search options.
        /// </summary>
        /// <param name="model">The view model containing search and filter criteria.</param>
        /// <returns>The status page view with filtered results.</returns>
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

        /// <summary>
        /// Displays detailed information for a specific service request.
        /// </summary>
        /// <param name="id">The unique identifier of the report.</param>
        /// <returns>The detailed status view if the report exists; otherwise, a 404 Not Found result.</returns>
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

        #endregion

        #region Advanced Views

        /// <summary>
        /// Displays all service requests sorted by creation date using a Binary Search Tree (BST).
        /// </summary>
        /// <returns>The tree view with sorted service requests.</returns>
        [HttpGet]
        [Route("Reports/TreeView")]
        public async Task<IActionResult> TreeView()
        {
            var sortedReports = await _reportService.ListReportsSortedByDateAsync();
            var viewModels = sortedReports.Select(ReportMapper.ToServiceRequestStatusViewModel).ToList();
            return View("TreeView", viewModels);
        }

        /// <summary>
        /// Displays the top N most urgent service requests using a MinHeap (priority queue).
        /// </summary>
        /// <param name="count">The number of urgent requests to display (default is 5).</param>
        /// <returns>The priority queue view with urgent service requests.</returns>
        [HttpGet]
        [Route("Reports/PriorityQueue")]
        public async Task<IActionResult> PriorityQueue(int count = 5)
        {
            var urgentReports = await _reportService.GetTopUrgentReportsAsync(count);
            var viewModels = urgentReports.Select(ReportMapper.ToServiceRequestStatusViewModel).ToList();
            return View("PriorityQueue", viewModels);
        }

        /// <summary>
        /// Visualizes related service requests using a graph traversal (BFS).
        /// </summary>
        /// <param name="id">The root request ID to visualize from.</param>
        /// <returns>The graph view with related service requests.</returns>
        [HttpGet]
        [Route("Reports/GraphView/{id}")]
        public async Task<IActionResult> GraphView(Guid id)
        {
            var relatedReports = await _reportService.GetRelatedRequestsByGraphAsync(id);
            var viewModels = relatedReports.Select(ReportMapper.ToServiceRequestStatusViewModel).ToList();
            return View("GraphView", viewModels);
        }

        #endregion
    }
}
