using Municipality_Application.Interfaces;
using Municipality_Application.Models;
using System.Collections.Concurrent;

namespace Municipality_Application.Data.InMemory
{
    /// <summary>
    /// In-memory implementation of <see cref="IReportRepository"/> for managing report, address, and attachment data during application runtime.
    /// </summary>
    public class InMemoryReportRepository : IReportRepository
    {
        private readonly ConcurrentDictionary<Guid, Report> _reports = new();
        private readonly ConcurrentDictionary<Guid, List<Attachment>> _attachments = new();
        private readonly ConcurrentDictionary<int, Address> _addresses = new();
        private readonly ConcurrentDictionary<int, Category> _categories = new();
        private int _addressIdCounter = 1;

        /// <summary>
        /// Adds a new report to the in-memory store, including its address.
        /// </summary>
        /// <param name="report">The report to add.</param>
        /// <returns>The added <see cref="Report"/> entity.</returns>
        public Task<Report> AddReportAsync(Report report)
        {
            if (report.Id == Guid.Empty)
                report.Id = Guid.NewGuid();

            // Handle Address
            if (report.Address != null)
            {
                // Assign a new AddressId if not set
                if (report.Address.Id == 0)
                {
                    report.Address.Id = _addressIdCounter++;
                }
                _addresses[report.Address.Id] = report.Address;
                report.AddressId = report.Address.Id;
            }

            // Handle Category
            if (report.CategoryId != 0)
                report.Category = _categories.TryGetValue(report.CategoryId, out var cat) ? cat : null;

            if (report.Attachments != null && report.Attachments.Any())
                _attachments[report.Id] = report.Attachments.ToList();

            _reports[report.Id] = report;
            return Task.FromResult(report);
        }

        /// <summary>
        /// Retrieves a report by its unique identifier, including its address and attachments.
        /// </summary>
        /// <param name="id">The report's unique identifier.</param>
        /// <returns>The <see cref="Report"/> entity if found; otherwise, null.</returns>
        public Task<Report?> GetReportByIdAsync(Guid id)
        {
            _reports.TryGetValue(id, out var report);
            if (report != null)
            {
                // Attachments
                if (_attachments.TryGetValue(id, out var attachments))
                    report.Attachments = attachments;
                else
                    report.Attachments = new List<Attachment>();

                // Address
                if (_addresses.TryGetValue(report.AddressId, out var address))
                    report.Address = address;
                else
                    report.Address = null!;

                // Category
                if (report.CategoryId != 0)
                    report.Category = _categories.TryGetValue(report.CategoryId, out var cat) ? cat : null;
            }
            return Task.FromResult(report);
        }

        /// <summary>
        /// Retrieves all reports from the in-memory store, including their addresses and attachments.
        /// </summary>
        /// <returns>A list of all <see cref="Report"/> entities.</returns>
        public Task<IEnumerable<Report>> GetAllReportsAsync()
        {
            var reports = _reports.Values.ToList();
            foreach (var report in reports)
            {
                // Attachments
                if (_attachments.TryGetValue(report.Id, out var attachments))
                    report.Attachments = attachments;
                else
                    report.Attachments = new List<Attachment>();

                // Address
                if (_addresses.TryGetValue(report.AddressId, out var address))
                    report.Address = address;
                else
                    report.Address = null!;

                // Category
                if (report.CategoryId != 0)
                    report.Category = _categories.TryGetValue(report.CategoryId, out var cat) ? cat : null;
            }
            return Task.FromResult(reports.AsEnumerable());
        }

        /// <summary>
        /// Updates an existing report in the in-memory store, including its address.
        /// </summary>
        /// <param name="report">The report with updated data.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public Task<bool> UpdateReportAsync(Report report)
        {
            if (!_reports.ContainsKey(report.Id))
                return Task.FromResult(false);

            // Update Address
            if (report.Address != null)
            {
                if (report.Address.Id == 0)
                {
                    report.Address.Id = _addressIdCounter++;
                }
                _addresses[report.Address.Id] = report.Address;
                report.AddressId = report.Address.Id;
            }

            // Update Category
            if (report.CategoryId != 0)
                report.Category = _categories.TryGetValue(report.CategoryId, out var cat) ? cat : null;

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
            var removed = _reports.TryRemove(id, out var report);
            _attachments.TryRemove(id, out _);

            // Optionally remove the address if not referenced by any other report
            if (report != null && report.AddressId != 0)
            {
                bool addressInUse = _reports.Values.Any(r => r.AddressId == report.AddressId && r.Id != id);
                if (!addressInUse)
                {
                    _addresses.TryRemove(report.AddressId, out _);
                }
            }

            return Task.FromResult(removed);
        }

        /// <summary>
        /// Retrieves filtered reports from the in-memory store, including their addresses and attachments.
        /// </summary>
        public Task<IEnumerable<Report>> GetFilteredReportsAsync(string? searchReportId, string? searchTitle, string? searchArea, DateTime? startDate, DateTime? endDate, int? categoryId, string? status)
        {
            var reports = _reports.Values.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(searchTitle))
                reports = reports.Where(r => r.Description.Contains(searchTitle, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(searchArea))
                reports = reports.Where(r =>
                    r.Address != null &&
                    (
                        (r.Address.Street != null && r.Address.Street.Contains(searchArea, StringComparison.OrdinalIgnoreCase)) ||
                        (r.Address.City != null && r.Address.City.Contains(searchArea, StringComparison.OrdinalIgnoreCase)) ||
                        (r.Address.Province != null && r.Address.Province.Contains(searchArea, StringComparison.OrdinalIgnoreCase)) ||
                        (r.Address.FormattedAddress != null && r.Address.FormattedAddress.Contains(searchArea, StringComparison.OrdinalIgnoreCase))
                    )
                );

            if (startDate.HasValue)
                reports = reports.Where(r => r.ReportedAt >= startDate.Value);

            if (endDate.HasValue)
                reports = reports.Where(r => r.ReportedAt <= endDate.Value);

            // Ensure addresses, attachments, and categories are populated
            foreach (var report in reports)
            {
                // Attachments
                if (_attachments.TryGetValue(report.Id, out var attachments))
                    report.Attachments = attachments;
                else
                    report.Attachments = new List<Attachment>();

                // Address
                if (_addresses.TryGetValue(report.AddressId, out var address))
                    report.Address = address;
                else
                    report.Address = null!;

                // Category
                if (report.CategoryId != 0)
                    report.Category = _categories.TryGetValue(report.CategoryId, out var cat) ? cat : null;
            }

            return Task.FromResult(reports);
        }

        /// <summary>
        /// Sets all reports in the repository, used by the data seeder.
        /// </summary>
        /// <param name="reports">The reports to set.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task SetAllReportsAsync(IEnumerable<Report> reports)
        {
            _reports.Clear();
            _attachments.Clear();
            _addresses.Clear();
            _addressIdCounter = 1;

            foreach (var report in reports)
            {
                if (report.Id == Guid.Empty)
                    report.Id = Guid.NewGuid();

                if (report.Address != null)
                {
                    if (report.Address.Id == 0)
                        report.Address.Id = _addressIdCounter++;
                    else
                        _addressIdCounter = Math.Max(_addressIdCounter, report.Address.Id + 1);

                    _addresses[report.Address.Id] = report.Address;
                    report.AddressId = report.Address.Id;
                }

                // Handle Category
                if (report.CategoryId != 0)
                    report.Category = _categories.TryGetValue(report.CategoryId, out var cat) ? cat : null;

                if (report.Attachments != null && report.Attachments.Any())
                    _attachments[report.Id] = report.Attachments.ToList();

                _reports[report.Id] = report;
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets all categories in the repository, used by the data seeder.
        /// </summary>
        /// <param name="categories">The categories to set.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task SetAllCategoriesAsync(IEnumerable<Category> categories)
        {
            _categories.Clear();
            foreach (var category in categories)
            {
                _categories[category.Id] = category;
            }
            return Task.CompletedTask;
        }
    }
}