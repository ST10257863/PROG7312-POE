using Municipality_Application.Models;

namespace Municipality_Application.Interfaces.Service
{
    /// <summary>
    /// Provides higher-level report operations such as submission, retrieval, modification, and removal.
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Submits a new report with optional file attachments.
        /// </summary>
        /// <param name="report">The report to submit.</param>
        /// <param name="files">A list of files to attach to the report.</param>
        /// <returns>The submitted <see cref="Report"/> object.</returns>
        Task<Report> SubmitReportAsync(Report report, List<IFormFile> files);

        /// <summary>
        /// Retrieves detailed information for a specific report.
        /// </summary>
        /// <param name="id">The report's unique identifier.</param>
        /// <returns>The <see cref="Report"/> object if found; otherwise, null.</returns>
        Task<Report?> GetReportDetailsAsync(Guid id);

        /// <summary>
        /// Lists all reports.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="Report"/> objects.</returns>
        Task<IEnumerable<Report>> ListReportsAsync();

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
        Task<IEnumerable<Report>> ListReportsFilteredAsync(string? searchTitle, string? searchArea, DateTime? startDate, DateTime? endDate, int? categoryId, string? status);
    }
}