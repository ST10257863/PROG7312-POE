using Municipality_Application.Interfaces;
using Municipality_Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Municipality_Application.Data.EF
{
    /// <summary>
    /// Entity Framework Core implementation of <see cref="IReportRepository"/> for managing report data in the database.
    /// </summary>
    public class EfReportRepository : IReportRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<EfReportRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EfReportRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The application's database context.</param>
        /// <param name="logger">The logger for performance instrumentation.</param>
        public EfReportRepository(AppDbContext dbContext, ILogger<EfReportRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Adds a new report to the database, including its address.
        /// </summary>
        /// <param name="report">The report to add.</param>
        /// <returns>The added <see cref="Report"/> entity.</returns>
        public async Task<Report> AddReportAsync(Report report)
        {
            // Ensure Address is added and tracked
            if (report.Address != null)
            {
                if (report.Address.Id == 0)
                {
                    _dbContext.Addresses.Add(report.Address);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    // Attach if not already tracked
                    if (!_dbContext.Addresses.Local.Any(a => a.Id == report.Address.Id))
                        _dbContext.Addresses.Attach(report.Address);
                }
                report.AddressId = report.Address.Id;
            }

            _dbContext.Reports.Add(report);
            await _dbContext.SaveChangesAsync();
            return report;
        }

        /// <summary>
        /// Retrieves a report by its unique identifier, including its address.
        /// </summary>
        /// <param name="id">The report's unique identifier.</param>
        /// <returns>The <see cref="Report"/> entity if found; otherwise, null.</returns>
        public async Task<Report> GetReportByIdAsync(Guid id)
        {
            return await _dbContext.Reports
                .Include(r => r.Attachments)
                .Include(r => r.Category)
                .Include(r => r.User)
                .Include(r => r.Address)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        /// <summary>
        /// Retrieves all reports from the database, including their addresses.
        /// </summary>
        /// <returns>A list of all <see cref="Report"/> entities.</returns>
        public async Task<IEnumerable<Report>> GetAllReportsAsync()
        {
            return await _dbContext.Reports
                .Include(r => r.Attachments)
                .Include(r => r.Category)
                .Include(r => r.User)
                .Include(r => r.Address)
                .ToListAsync();
        }

        /// <summary>
        /// Updates an existing report in the database, including its address.
        /// </summary>
        /// <param name="report">The report with updated data.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public async Task<bool> UpdateReportAsync(Report report)
        {
            var existingReport = await _dbContext.Reports
                .Include(r => r.Attachments)
                .Include(r => r.Address)
                .FirstOrDefaultAsync(r => r.Id == report.Id);

            if (existingReport == null)
                return false;

            // Update Address
            if (report.Address != null)
            {
                if (existingReport.Address == null || existingReport.Address.Id != report.Address.Id)
                {
                    // New address, add it
                    _dbContext.Addresses.Add(report.Address);
                    await _dbContext.SaveChangesAsync();
                    report.AddressId = report.Address.Id;
                }
                else
                {
                    // Update existing address
                    _dbContext.Entry(existingReport.Address).CurrentValues.SetValues(report.Address);
                }
            }

            _dbContext.Entry(existingReport).CurrentValues.SetValues(report);

            if (report.Attachments != null)
            {
                _dbContext.Attachments.RemoveRange(existingReport.Attachments ?? new List<Attachment>());
                foreach (var attachment in report.Attachments)
                {
                    attachment.ReportId = report.Id;
                }
                _dbContext.Attachments.AddRange(report.Attachments);
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Deletes a report and its attachments from the database.
        /// </summary>
        /// <param name="id">The report's unique identifier.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        public async Task<bool> DeleteReportAsync(Guid id)
        {
            var report = await _dbContext.Reports
                .Include(r => r.Attachments)
                .Include(r => r.Address)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
                return false;

            _dbContext.Attachments.RemoveRange(report.Attachments ?? new List<Attachment>());
            _dbContext.Reports.Remove(report);

            // Optionally, remove the address if not referenced by any other report
            if (report.AddressId != 0)
            {
                bool addressInUse = await _dbContext.Reports.AnyAsync(r => r.AddressId == report.AddressId && r.Id != id);
                if (!addressInUse && report.Address != null)
                {
                    _dbContext.Addresses.Remove(report.Address);
                }
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Retrieves filtered reports from the database, including their addresses.
        /// </summary>
        public async Task<IEnumerable<Report>> GetFilteredReportsAsync(
            string? searchReportId,
            string? searchTitle,
            string? searchArea,
            DateTime? startDate,
            DateTime? endDate,
            int? categoryId,
            string? status)
        {
            var stopwatch = Stopwatch.StartNew();

            var query = _dbContext.Reports
                .AsNoTracking()
                .Include(r => r.Attachments)
                .Include(r => r.Category)
                .Include(r => r.User)
                .Include(r => r.Address)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchReportId))
            {
                if (Guid.TryParse(searchReportId, out var reportGuid))
                {
                    query = query.Where(r => r.Id == reportGuid);
                }
                else
                {
                    query = query.Where(r => r.Id.ToString().Contains(searchReportId));
                }
            }

            if (!string.IsNullOrWhiteSpace(searchTitle))
                query = query.Where(r => r.Description.Contains(searchTitle));

            if (!string.IsNullOrWhiteSpace(searchArea))
                query = query.Where(r =>
                    r.Address != null &&
                    (
                        (r.Address.Street != null && r.Address.Street.Contains(searchArea)) ||
                        (r.Address.City != null && r.Address.City.Contains(searchArea)) ||
                        (r.Address.Province != null && r.Address.Province.Contains(searchArea)) ||
                        (r.Address.FormattedAddress != null && r.Address.FormattedAddress.Contains(searchArea))
                    )
                );

            if (startDate.HasValue)
                query = query.Where(r => r.ReportedAt >= startDate.Value);
            if (endDate.HasValue)
            {
                var nextDay = endDate.Value.Date.AddDays(1);
                query = query.Where(r => r.ReportedAt < nextDay);
            }

            if (categoryId.HasValue)
                query = query.Where(r => r.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<IssueStatus>(status, out var parsedStatus))
                query = query.Where(r => r.Status == parsedStatus);

            var result = await query.OrderByDescending(r => r.ReportedAt).ToListAsync();

            stopwatch.Stop();
            _logger.LogInformation("GetFilteredReportsAsync executed in {Elapsed} ms", stopwatch.ElapsedMilliseconds);

            return result;
        }

        /// <summary>
        /// Retrieves filtered reports from the database as lightweight DTOs (ReportSummaryDto) using projection.
        /// Only fetches necessary columns for summary views.
        /// </summary>
        public async Task<IEnumerable<ReportSummaryDto>> GetFilteredReportsSummaryAsync(
            string? searchReportId,
            string? searchTitle,
            string? searchArea,
            DateTime? startDate,
            DateTime? endDate,
            int? categoryId,
            string? status)
        {
            var stopwatch = Stopwatch.StartNew();

            var query = _dbContext.Reports
                .AsNoTracking()
                .Include(r => r.Category)
                .Include(r => r.Address)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchReportId))
            {
                if (Guid.TryParse(searchReportId, out var reportGuid))
                {
                    query = query.Where(r => r.Id == reportGuid);
                }
                else
                {
                    query = query.Where(r => r.Id.ToString().Contains(searchReportId));
                }
            }

            if (!string.IsNullOrWhiteSpace(searchTitle))
                query = query.Where(r => r.Description.Contains(searchTitle));

            if (!string.IsNullOrWhiteSpace(searchArea))
                query = query.Where(r =>
                    r.Address != null &&
                    (
                        (r.Address.Street != null && r.Address.Street.Contains(searchArea)) ||
                        (r.Address.City != null && r.Address.City.Contains(searchArea)) ||
                        (r.Address.Province != null && r.Address.Province.Contains(searchArea)) ||
                        (r.Address.FormattedAddress != null && r.Address.FormattedAddress.Contains(searchArea))
                    )
                );

            if (startDate.HasValue)
                query = query.Where(r => r.ReportedAt >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(r => r.ReportedAt <= endDate.Value);

            if (categoryId.HasValue)
                query = query.Where(r => r.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<IssueStatus>(status, out var parsedStatus))
                query = query.Where(r => r.Status == parsedStatus);

            var result = await query
                .OrderByDescending(r => r.ReportedAt)
                .Select(r => new ReportSummaryDto
                {
                    Id = r.Id,
                    Description = r.Description,
                    CategoryName = r.Category != null ? r.Category.Name : string.Empty,
                    AddressFormatted = r.Address != null ? r.Address.FormattedAddress : string.Empty,
                    ReportedAt = r.ReportedAt,
                    Status = r.Status
                })
                .ToListAsync();

            stopwatch.Stop();
            _logger.LogInformation("GetFilteredReportsSummaryAsync executed in {Elapsed} ms", stopwatch.ElapsedMilliseconds);

            return result;
        }

        /// <summary>
        /// Lightweight DTO for report summary projection.
        /// </summary>
        public class ReportSummaryDto
        {
            public Guid Id { get; set; }
            public string Description { get; set; } = string.Empty;
            public string CategoryName { get; set; } = string.Empty;
            public string AddressFormatted { get; set; } = string.Empty;
            public DateTime ReportedAt { get; set; }
            public IssueStatus Status { get; set; }
        }
    }
}