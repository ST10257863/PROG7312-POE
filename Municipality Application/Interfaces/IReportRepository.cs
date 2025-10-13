using Municipality_Application.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Municipality_Application.Interfaces
{
    public interface IReportRepository
    {
        Task<Report> AddReportAsync(Report report);
        Task<Report> GetReportByIdAsync(Guid id);
        Task<IEnumerable<Report>> GetAllReportsAsync();
        Task<bool> UpdateReportAsync(Report report);
        Task<bool> DeleteReportAsync(Guid id);
    }
}