using Municipality_Application.Interfaces;
using Municipality_Application.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Municipality_Application.Data
{
    public class InMemoryReportRepository : IReportRepository
    {
        private readonly ConcurrentDictionary<Guid, Report> _reports = new ConcurrentDictionary<Guid, Report>();
        private readonly ConcurrentDictionary<Guid, List<Attachment>> _attachments = new ConcurrentDictionary<Guid, List<Attachment>>();
        private Queue<Guid> _reportQueue = new Queue<Guid>();

        public Task<Report> AddReportAsync(Report report, List<IFormFile> files)
        {
            const long MaxFileSize = 5 * 1024 * 1024; // 5MB

            if (report.Id == Guid.Empty)
            {
                report.Id = Guid.NewGuid();
            }

            // Handle attachments
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

                        // Store file in memory as base64 string (simulate file storage)
                        using var ms = new MemoryStream();
                        file.CopyTo(ms);
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

            // Store attachments in the in-memory dictionary
            if (attachments.Any())
            {
                _attachments[report.Id] = attachments;
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
                // Attachments retrieval
                if (_attachments.TryGetValue(id, out var attachments))
                {
                    report.Attachments = attachments;
                }
                else
                {
                    report.Attachments = new List<Attachment>();
                }
                return Task.FromResult(report);
            }

            return Task.FromResult<Report>(null);
        }

        public Task<IEnumerable<Report>> GetAllReportsAsync()
        {
            var reports = _reports.Values.ToList();
            foreach (var report in reports)
            {
                if (_attachments.TryGetValue(report.Id, out var attachments))
                {
                    report.Attachments = attachments;
                }
                else
                {
                    report.Attachments = new List<Attachment>();
                }
            }
            return Task.FromResult(reports.AsEnumerable());
        }

        public Task<bool> UpdateReportAsync(Report report)
        {
            if (_reports.TryGetValue(report.Id, out Report existingReport))
            {
                // Update attachments
                if (report.Attachments != null)
                {
                    foreach (var attachment in report.Attachments)
                    {
                        attachment.ReportId = report.Id;
                    }
                    _attachments[report.Id] = report.Attachments.ToList();
                }
                _reports[report.Id] = report;
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Task<bool> DeleteReportAsync(Guid id)
        {
            if (_reports.TryRemove(id, out _))
            {
                _attachments.TryRemove(id, out _);
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