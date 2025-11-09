using Municipality_Application.Models;

namespace Municipality_Application.Interfaces
{
    /// <summary>
    /// Provides methods for managing and retrieving report data.
    /// </summary>
    public interface IReportRepository
    {
        /// <summary>
        /// Adds a new report to the data store.
        /// </summary>
        /// <param name="report">The report to add.</param>
        /// <returns>The added <see cref="Report"/> object.</returns>
        Task<Report> AddReportAsync(Report report);

        /// <summary>
        /// Retrieves a report by its unique identifier.
        /// </summary>
        /// <param name="id">The report's unique identifier.</param>
        /// <returns>The <see cref="Report"/> object if found; otherwise, null.</returns>
        Task<Report> GetReportByIdAsync(Guid id);

        /// <summary>
        /// Retrieves all reports.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="Report"/> objects.</returns>
        Task<IEnumerable<Report>> GetAllReportsAsync();

        /// <summary>
        /// Updates an existing report.
        /// </summary>
        /// <param name="report">The report with updated data.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        Task<bool> UpdateReportAsync(Report report);

        /// <summary>
        /// Deletes a report by its unique identifier.
        /// </summary>
        /// <param name="id">The report's unique identifier.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        Task<bool> DeleteReportAsync(Guid id);
        Task<IEnumerable<Report>> GetFilteredReportsAsync(string? searchTitle, string? searchArea, DateTime? startDate, DateTime? endDate);
    }
}       