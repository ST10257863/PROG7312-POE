using Municipality_Application.Interfaces;
using Municipality_Application.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Municipality_Application.Data
{
    public class InMemoryReportRepository : IReportRepository
    {
        private readonly ConcurrentDictionary<Guid, Report> _reports = new ConcurrentDictionary<Guid, Report>();
        private Queue<Guid> _reportQueue = new Queue<Guid>();

        public Task<Report> AddReportAsync(Report report)
        {
            if (report.Id == Guid.Empty)
            {
                report.Id = Guid.NewGuid();
            }

            if (_reports.TryAdd(report.Id, report))
            {
                _reportQueue.Enqueue(report.Id);
                return Task.FromResult(report);
            }

            throw new Exception("Failed to add report");
        }
        
        public Task<Report> GetReportByIdAsync(Guid id)
        {
            if (_reports.TryGetValue(id, out Report report))
            {
                return Task.FromResult(report);
            }

            return Task.FromResult<Report>(null);
        }

        public Task<IEnumerable<Report>> GetAllReportsAsync()
        {
            return Task.FromResult(_reports.Values.AsEnumerable());
        }

        public Task<bool> UpdateReportAsync(Report report)
        {
            if (_reports.TryGetValue(report.Id, out Report existingReport))
            {
                _reports[report.Id] = report;
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Task<bool> DeleteReportAsync(Guid id)
        {
            if (_reports.TryRemove(id, out _))
            {
                var filteredQueue = _reportQueue.Where(x => x != id).ToList();
                _reportQueue.Clear();
                foreach (var item in filteredQueue)
                {
                    _reportQueue.Enqueue(item);
                }
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}