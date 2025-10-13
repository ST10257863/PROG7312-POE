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

        public async Task<Report> AddReportAsync(Report report, List<IFormFile> files)
        {
            const long MaxFileSize = 5 * 1024 * 1024; // 5MB

            if (report.Id == Guid.Empty)
            {
                report.Id = Guid.NewGuid();
            }

            var attachments = new List<Attachment>();
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        if (file.Length > MaxFileSize)
                        {
                            throw new Exception($"File '{file.FileName}' exceeds the 5MB size limit.");
                        }

                        using var ms = new MemoryStream();
                        await file.CopyToAsync(ms);
                        var fileBytes = ms.ToArray();
                        var base64 = Convert.ToBase64String(fileBytes);
                        var dataUrl = $"data:{file.ContentType};base64,{base64}";

                        attachments.Add(new Attachment
                        {
                            Id = Guid.NewGuid(),
                            ReportId = report.Id,
                            FileType = file.ContentType,
                            FileSize = file.Length,
                            FilePath = dataUrl,
                            FileName = file.FileName
                        });
                    }
                }
            }

            report.Attachments = attachments;

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

            // Update scalar properties
            _dbContext.Entry(existingReport).CurrentValues.SetValues(report);

            // Update attachments
            if (report.Attachments != null)
            {
                // Remove old attachments
                _dbContext.Attachments.RemoveRange(existingReport.Attachments ?? new List<Attachment>());
                // Add new attachments
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