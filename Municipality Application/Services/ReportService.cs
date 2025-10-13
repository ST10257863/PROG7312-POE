using Municipality_Application.Interfaces;
using Municipality_Application.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Municipality_Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        // Example data structures for business logic
        private readonly Queue<Report> _reportQueue = new();
        private readonly Stack<Report> _reportStack = new();
        private readonly HashSet<int> _categorySet = new();
        private readonly Dictionary<Guid, Report> _reportDictionary = new();

        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        private async Task OrganizeReportsAsync()
        {
            var reports = await _reportRepository.GetAllReportsAsync();

            _reportQueue.Clear();
            _reportStack.Clear();
            _categorySet.Clear();
            _reportDictionary.Clear();

            foreach (var report in reports)
            {
                _reportQueue.Enqueue(report);
                _reportStack.Push(report);
                _categorySet.Add(report.CategoryId);
                _reportDictionary[report.Id] = report;
            }
        }

        public async Task<Report> SubmitReportAsync(Report report, List<IFormFile> files)
        {
            return await _reportRepository.AddReportAsync(report, files);
        }

        public async Task<Report?> GetReportDetailsAsync(Guid id)
        {
            await OrganizeReportsAsync();
            _reportDictionary.TryGetValue(id, out var report);
            return report;
        }

        public async Task<IEnumerable<Report>> ListReportsAsync()
        {
            await OrganizeReportsAsync();
            return _reportQueue.ToList();
        }

        public async Task<bool> ModifyReportAsync(Report report)
        {
            return await _reportRepository.UpdateReportAsync(report);
        }

        public async Task<bool> RemoveReportAsync(Guid id)
        {
            return await _reportRepository.DeleteReportAsync(id);
        }
    }
}
