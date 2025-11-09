using Municipality_Application.Interfaces;
using Municipality_Application.Interfaces.Service;
using Municipality_Application.Models;

namespace Municipality_Application.Services
{
    /// <summary>
    /// Provides higher-level report operations such as submission, retrieval, modification, and removal.
    /// Organizes report data using various data structures for efficient access and demonstration purposes.
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        private readonly Queue<Report> _reportQueue = new();
        private readonly Stack<Report> _reportStack = new();
        private readonly HashSet<int> _categorySet = new();
        private readonly Dictionary<Guid, Report> _reportDictionary = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportService"/> class.
        /// </summary>
        /// <param name="reportRepository">The report repository instance.</param>
        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        /// <summary>
        /// Loads and organizes reports into internal data structures for efficient access.
        /// </summary>
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

        /// <summary>
        /// Processes file attachments for a report, converting them to base64 data URLs.
        /// </summary>
        /// <param name="reportId">The unique identifier of the report.</param>
        /// <param name="files">A list of files to attach.</param>
        /// <returns>A list of processed <see cref="Attachment"/> objects.</returns>
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

        /// <summary>
        /// Submits a new report with optional file attachments.
        /// Latitude and Longitude should already be set on the report model.
        /// </summary>
        /// <param name="report">The report to submit.</param>
        /// <param name="files">A list of files to attach to the report.</param>
        /// <returns>The submitted <see cref="Report"/> object.</returns>
        public async Task<Report> SubmitReportAsync(Report report, List<IFormFile> files)
        {
            if (report.Id == Guid.Empty)
            {
                report.Id = Guid.NewGuid();
            }

            report.Attachments = await ProcessAttachmentsAsync(report.Id, files);
            // Latitude and Longitude are already set by the controller
            return await _reportRepository.AddReportAsync(report);
        }

        public async Task<Report?> GetReportDetailsAsync(Guid id)
        {
            await OrganizeReportsAsync();
            _reportDictionary.TryGetValue(id, out var report);
            return report;
        }

        /// <summary>
        /// Lists all reports.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="Report"/> objects.</returns>
        public async Task<IEnumerable<Report>> ListReportsAsync()
        {
            await OrganizeReportsAsync();
            return _reportQueue.ToList();
        }

        /// <summary>
        /// Modifies an existing report.
        /// </summary>
        /// <param name="report">The report with updated data.</param>
        /// <returns>True if the modification was successful; otherwise, false.</returns>
        public async Task<bool> ModifyReportAsync(Report report)
        {
            return await _reportRepository.UpdateReportAsync(report);
        }

        /// <summary>
        /// Removes a report by its unique identifier.
        /// </summary>
        /// <param name="id">The report's unique identifier.</param>
        /// <returns>True if the removal was successful; otherwise, false.</returns>
        public async Task<bool> RemoveReportAsync(Guid id)
        {
            return await _reportRepository.DeleteReportAsync(id);
        }
    }
}
