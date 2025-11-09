using Municipality_Application.Models;
using Municipality_Application.ViewModels;

namespace Municipality_Application.Mappers
{
    public static class ReportMapper
    {
        public static ServiceRequestStatusViewModel ToViewModel(Report report)
        {
            return new ServiceRequestStatusViewModel
            {
                Id = report.Id,
                Description = report.Description,
                Address = report.Address,
                ReportedAt = report.ReportedAt,
                Status = report.Status.ToString(),
                Category = report.Category?.Name
            };
        }
    }
}