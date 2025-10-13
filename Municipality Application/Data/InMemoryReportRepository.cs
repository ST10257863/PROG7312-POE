using Municipality_Application.Interfaces;
using Municipality_Application.Models;
using System.Collections.Concurrent;

namespace Municipality_Application.Data
{
    public class InMemoryReportRepository : IReportRepository
    {
        private readonly ConcurrentDictionary<Guid, Report> _reports = new();
        private readonly ConcurrentDictionary<Guid, List<Attachment>> _attachments = new();

        public Task<Report> AddReportAsync(Report report)
        {
            if (report.Id == Guid.Empty)
                report.Id = Guid.NewGuid();

            if (report.Attachments != null && report.Attachments.Any())
                _attachments[report.Id] = report.Attachments.ToList();

            _reports[report.Id] = report;
            return Task.FromResult(report);
        }

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

        public Task<bool> UpdateReportAsync(Report report)
        {
            if (!_reports.ContainsKey(report.Id))
                return Task.FromResult(false);

            _reports[report.Id] = report;
            if (report.Attachments != null)
                _attachments[report.Id] = report.Attachments.ToList();

            return Task.FromResult(true);
        }

        public Task<bool> DeleteReportAsync(Guid id)
        {
            var removed = _reports.TryRemove(id, out _);
            _attachments.TryRemove(id, out _);
            return Task.FromResult(removed);
        }
    }
}