using Municipality_Application.Interfaces;
using Municipality_Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Municipality_Application.Data
{
    public class EfReportRepository : IReportRepository
    {
        private readonly AppDbContext _dbContext;

        public EfReportRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Report> AddReportAsync(Report report)
        {
            _dbContext.Reports.Add(report);
            await _dbContext.SaveChangesAsync();
            return report;
        }

        public async Task<Report> GetReportByIdAsync(Guid id)
        {
            return await _dbContext.Reports
                .Include(r => r.Attachments)
                .Include(r => r.Category)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Report>> GetAllReportsAsync()
        {
            return await _dbContext.Reports
                .Include(r => r.Attachments)
                .Include(r => r.Category)
                .Include(r => r.User)
                .ToListAsync();
        }

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