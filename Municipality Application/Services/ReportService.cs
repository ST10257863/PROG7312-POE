using Municipality_Application.Interfaces;
using Municipality_Application.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace Municipality_Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

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

        private async Task<List<Attachment>> ProcessAttachmentsAsync(Guid reportId, List<IFormFile> files)
        {
            const long MaxFileSize = 5 * 1024 * 1024; // 5MB
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
                            ReportId = reportId,
                            FileType = file.ContentType,
                            FileSize = file.Length,
                            FilePath = dataUrl,
                            FileName = file.FileName
                        });
                    }
                }
            }

            return attachments;
        }

        public async Task<Report> SubmitReportAsync(Report report, List<IFormFile> files)
        {
            if (report.Id == Guid.Empty)
            {
                report.Id = Guid.NewGuid();
            }

            report.Attachments = await ProcessAttachmentsAsync(report.Id, files);
            return await _reportRepository.AddReportAsync(report);
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
