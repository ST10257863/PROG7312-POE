using Municipality_Application.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Municipality_Application.Interfaces.Service
{
    /// <summary>
    /// Provides methods for managing and retrieving report data, including advanced data structure integrations for efficient access and filtering.
    /// </summary>
    public interface IReportService
    {
        #region Report Submission and Modification

        /// <summary>
        /// Submits a new report with optional file attachments.
        /// </summary>
        /// <param name="report">The report to submit.</param>
        /// <param name="files">A list of files to attach to the report.</param>
        /// <returns>The submitted <see cref="Report"/> object.</returns>
        Task<Report> SubmitReportAsync(Report report, List<IFormFile> files);

        /// <summary>
        /// Modifies an existing report.
        /// </summary>
        /// <param name="report">The report with updated data.</param>
        /// <returns>True if the modification was successful; otherwise, false.</returns>
        Task<bool> ModifyReportAsync(Report report);

        /// <summary>
        /// Removes a report by its unique identifier.
        /// </summary>
        /// <param name="id">The report's unique identifier.</param>
        /// <returns>True if the removal was successful; otherwise, false.</returns>
        Task<bool> RemoveReportAsync(Guid id);

        #endregion

        #region Report Retrieval

        /// <summary>
        /// Retrieves detailed information for a specific report.
        /// </summary>
        /// <param name="id">The report's unique identifier.</param>
        /// <returns>The <see cref="Report"/> object if found; otherwise, null.</returns>
        Task<Report?> GetReportDetailsAsync(Guid id);

        /// <summary>
        /// Lists all reports, optionally forcing a refresh from the data source.
        /// </summary>
        /// <param name="forceRefresh">If true, forces a refresh from the data source; otherwise, may use cached data.</param>
        /// <returns>An enumerable collection of <see cref="Report"/> objects.</returns>
        Task<IEnumerable<Report>> ListReportsAsync(bool forceRefresh = false);

        /// <summary>
        /// Fetches a list of reports filtered by the specified criteria.
        /// </summary>
        /// <param name="searchReportId">Optional: Filter by report ID (partial match).</param>
        /// <param name="searchTitle">Optional: Filter by report title (partial match).</param>
        /// <param name="searchArea">Optional: Filter by area, suburb, or city (partial match).</param>
        /// <param name="startDate">Optional: Filter for reports submitted on or after this date.</param>
        /// <param name="endDate">Optional: Filter for reports submitted on or before this date.</param>
        /// <param name="categoryId">Optional: Filter by category ID.</param>
        /// <param name="status">Optional: Filter by report status.</param>
        /// <returns>An enumerable collection of filtered <see cref="Report"/> objects.</returns>
        Task<IEnumerable<Report>> ListReportsFilteredAsync(
            string? searchReportId,
            string? searchTitle,
            string? searchArea,
            DateTime? startDate,
            DateTime? endDate,
            int? categoryId,
            string? status);

        /// <summary>
        /// Gets a list of status options for use in dropdowns, based on the <see cref="IssueStatus"/> enum.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="SelectListItem"/> representing status options.</returns>
        IEnumerable<SelectListItem> GetIssueStatusSelectList();

        #endregion

        #region Advanced Data Structure Operations

        /// <summary>
        /// Returns all reports sorted by ReportedAt using a Binary Search Tree (BST) for efficient O(log n) search and retrieval.
        /// </summary>
        /// <returns>Sorted list of reports.</returns>
        Task<IEnumerable<Report>> ListReportsSortedByDateAsync();

        /// <summary>
        /// Searches reports in memory for those within the specified date range using a BST for O(log n) efficiency.
        /// </summary>
        /// <param name="start">Start of the date range (inclusive).</param>
        /// <param name="end">End of the date range (inclusive).</param>
        /// <returns>Reports within the date range.</returns>
        Task<IEnumerable<Report>> SearchReportsByDateRangeAsync(DateTime start, DateTime end);

        /// <summary>
        /// Returns the top N most urgent unresolved reports using a MinHeap for prioritization.
        /// Demonstrates O(log n) extraction of the most urgent unresolved requests.
        /// </summary>
        /// <param name="count">Number of urgent reports to return.</param>
        /// <returns>List of urgent reports.</returns>
        Task<IEnumerable<Report>> GetTopUrgentReportsAsync(int count = 5);

        /// <summary>
        /// Demonstrates graph traversal by finding related service requests using BFS from the given root request.
        /// Builds a graph where each node is a report, and edges connect reports with the same category or suburb.
        /// </summary>
        /// <param name="rootId">The root report ID to start traversal from.</param>
        /// <returns>All related reports discovered via BFS traversal.</returns>
        Task<IEnumerable<Report>> GetRelatedRequestsByGraphAsync(Guid rootId);

        #endregion
    }
}