using Municipality_Application.Interfaces;
using Municipality_Application.Models;
using System.Collections.Concurrent;

namespace Municipality_Application.Data.InMemory
{
    /// <summary>
    /// In-memory implementation of <see cref="IReportRepository"/> for managing report and attachment data during application runtime.
    /// </summary>
    public class InMemoryReportRepository : IReportRepository
    {
        private readonly ConcurrentDictionary<Guid, Report> _reports = new();
        private readonly ConcurrentDictionary<Guid, List<Attachment>> _attachments = new();

        /// <summary>
        /// Adds a new report to the in-memory store.
        /// </summary>
        /// <param name="report">The report to add.</param>
        /// <returns>The added <see cref="Report"/> entity.</returns>
        public Task<Report> AddReportAsync(Report report)
        {
            if (report.Id == Guid.Empty)
                report.Id = Guid.NewGuid();

            if (report.Attachments != null && report.Attachments.Any())
                _attachments[report.Id] = report.Attachments.ToList();

            _reports[report.Id] = report;
            return Task.FromResult(report);
        }

        /// <summary>
        /// Retrieves a report by its unique identifier.
        /// </summary>
        /// <param name="id">The report's unique identifier.</param>
        /// <returns>The <see cref="Report"/> entity if found; otherwise, null.</returns>
        public Task<Report> GetReportByIdAsync(Guid id)
        {
            _reports.TryGetValue(id, out var report);
            if (report != null)
            {
                if (_attachments.TryGetValue(id, out var attachments))
                    report.Attachments = attachments;
                else
                    report.Attachments = new List<Attachment>();
            }
            return Task.FromResult(report);
        }

        /// <summary>
        /// Retrieves all reports from the in-memory store.
        /// </summary>
        /// <returns>A list of all <see cref="Report"/> entities.</returns>
        public Task<IEnumerable<Report>> GetAllReportsAsync()
        {
            var reports = _reports.Values.ToList();
            foreach (var report in reports)
            {
                if (_attachments.TryGetValue(report.Id, out var attachments))
                    report.Attachments = attachments;
                else
                    report.Attachments = new List<Attachment>();
            }
            return Task.FromResult(reports.AsEnumerable());
        }

        /// <summary>
        /// Updates an existing report in the in-memory store.
        /// </summary>
        /// <param name="report">The report with updated data.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public Task<bool> UpdateReportAsync(Report report)
        {
            if (!_reports.ContainsKey(report.Id))
                return Task.FromResult(false);

            _reports[report.Id] = report;
            if (report.Attachments != null)
                _attachments[report.Id] = report.Attachments.ToList();

            return Task.FromResult(true);
        }

        /// <summary>
        /// Deletes a report and its attachments from the in-memory store.
        /// </summary>
        /// <param name="id">The report's unique identifier.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        public Task<bool> DeleteReportAsync(Guid id)
        {
            var removed = _reports.TryRemove(id, out _);
            _attachments.TryRemove(id, out _);
            return Task.FromResult(removed);
        }

        public Task<IEnumerable<Report>> GetFilteredReportsAsync(string? searchTitle, string? searchArea, DateTime? startDate, DateTime? endDate)
        {
            var reports = _reports.Values.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(searchTitle))
                reports = reports.Where(r => r.Description.Contains(searchTitle, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(searchArea))
                reports = reports.Where(r => r.Address.Contains(searchArea, StringComparison.OrdinalIgnoreCase));
            if (startDate.HasValue)
                reports = reports.Where(r => r.ReportedAt >= startDate.Value);
            if (endDate.HasValue)
                reports = reports.Where(r => r.ReportedAt <= endDate.Value);

            return Task.FromResult(reports);
        }
    }
}