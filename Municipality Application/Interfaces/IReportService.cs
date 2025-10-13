using Municipality_Application.Models;

namespace Municipality_Application.Interfaces
{
    public interface IReportService
    {
        Task<Report> SubmitReportAsync(Report report, List<IFormFile> files);
        Task<Report?> GetReportDetailsAsync(Guid id);
        Task<IEnumerable<Report>> ListReportsAsync();
        Task<bool> ModifyReportAsync(Report report);
        Task<bool> RemoveReportAsync(Guid id);
    }
}
