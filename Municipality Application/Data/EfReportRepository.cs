using Municipality_Application.Interfaces;
using Municipality_Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Municipality_Application.Data
{
    /// <summary>
    /// Entity Framework Core implementation of <see cref="IReportRepository"/> for managing report data in the database.
    /// </summary>
    public class EfReportRepository : IReportRepository
    {
        private readonly AppDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="EfReportRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The application's database context.</param>
        public EfReportRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Adds a new report to the database.
        /// </summary>
        /// <param name="report">The report to add.</param>
        /// <returns>The added <see cref="Report"/> entity.</returns>
        public async Task<Report> AddReportAsync(Report report)
        {
            _dbContext.Reports.Add(report);
            await _dbContext.SaveChangesAsync();
            return report;
        }

        /// <summary>
        /// Retrieves a report by its unique identifier.
        /// </summary>
        /// <param name="id">The report's unique identifier.</param>
        /// <returns>The <see cref="Report"/> entity if found; otherwise, null.</returns>
        public async Task<Report> GetReportByIdAsync(Guid id)
        {
            return await _dbContext.Reports
                .Include(r => r.Attachments)
                .Include(r => r.Category)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        /// <summary>
        /// Retrieves all reports from the database.
        /// </summary>
        /// <returns>A list of all <see cref="Report"/> entities.</returns>
        public async Task<IEnumerable<Report>> GetAllReportsAsync()
        {
            return await _dbContext.Reports
                .Include(r => r.Attachments)
                .Include(r => r.Category)
                .Include(r => r.User)
                .ToListAsync();
        }

        /// <summary>
        /// Updates an existing report in the database.
        /// </summary>
        /// <param name="report">The report with updated data.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public async Task<bool> UpdateReportAsync(Report report)
        {
            var existingReport = await _dbContext.Reports
                .Include(r => r.Attachments)
                .FirstOrDefaultAsync(r => r.Id == report.Id);

            if (existingReport == null)
                return false;

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
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
                return false;

            _dbContext.Attachments.RemoveRange(report.Attachments ?? new List<Attachment>());
            _dbContext.Reports.Remove(report);

            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}