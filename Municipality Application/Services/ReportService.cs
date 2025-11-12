using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Municipality_Application.Interfaces;
using Municipality_Application.Interfaces.Service;
using Municipality_Application.Models;
using Municipality_Application.Services.DataStructures;
using System.Linq.Expressions;

namespace Municipality_Application.Services
{
    /// <summary>
    /// Provides methods for managing and retrieving report data.
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<ReportService> _logger;

        // In-memory structures for fast access
        private BinarySearchTree<ReportByDateWrapper>? _reportBst;
        private MaxHeap<Report>? _recentHeap;
        private MinHeap<ReportPriorityWrapper>? _priorityHeap;

        private const string CacheKey = "RecentReportsCache";
        private const int MaxReports = 2000;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        public ReportService(
            IReportRepository reportRepository,
            IMemoryCache memoryCache,
            ILogger<ReportService> logger,
            IHostApplicationLifetime appLifetime)
        {
            _reportRepository = reportRepository;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        /// <summary>
        /// Fetches only the latest 2000 reports using projection, stores them in BST and Heap, and caches the dataset for 5 minutes.
        /// Set forceRefresh to true to reload from DB.
        /// </summary>
        /// <param name="forceRefresh">If true, reloads from DB even if cache is valid.</param>
        /// <returns>List of the latest reports.</returns>
        public async Task<IEnumerable<Report>> ListReportsAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && _memoryCache.TryGetValue(CacheKey, out List<Report> cachedReports))
            {
                // Always rebuild in-memory structures from cache
                BuildInMemoryStructures(cachedReports);
                return cachedReports;
            }

            // Fetch from DB
            var allReports = (await _reportRepository.GetAllReportsAsync())
                .OrderByDescending(r => r.ReportedAt)
                .Take(MaxReports)
                .ToList();

            BuildInMemoryStructures(allReports);
            _memoryCache.Set(CacheKey, allReports, CacheDuration);
            return allReports;
        }

        private void BuildInMemoryStructures(List<Report> reports)
        {
            _reportBst = new BinarySearchTree<ReportByDateWrapper>();
            foreach (var report in reports)
                _reportBst.Insert(new ReportByDateWrapper(report));

            _recentHeap = new MaxHeap<Report>(Comparer<Report>.Create(
                (a, b) => b.ReportedAt.CompareTo(a.ReportedAt)
            ));
            foreach (var report in reports)
                _recentHeap.Insert(report);

            _priorityHeap = new MinHeap<ReportPriorityWrapper>();
            foreach (var report in reports)
            {
                if (report.Status == IssueStatus.Reported)
                    _priorityHeap.Insert(new ReportPriorityWrapper(report));
            }
        }

        private async Task<List<Attachment>> ProcessAttachmentsAsync(Guid reportId, List<IFormFile> files)
        {
            const long MaxFileSize = 5 * 1024 * 1024;
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

        /// <inheritdoc/>
        public async Task<Report> SubmitReportAsync(Report report, List<IFormFile> files)
        {
            if (report.Id == Guid.Empty)
            {
                report.Id = Guid.NewGuid();
            }

            report.Attachments = await ProcessAttachmentsAsync(report.Id, files);
            return await _reportRepository.AddReportAsync(report);
        }

        /// <inheritdoc/>
        public async Task<Report?> GetReportDetailsAsync(Guid id)
        {
            return await _reportRepository.GetReportByIdAsync(id);
        }

        /// <inheritdoc/>
        public async Task<bool> ModifyReportAsync(Report report)
        {
            return await _reportRepository.UpdateReportAsync(report);
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveReportAsync(Guid id)
        {
            return await _reportRepository.DeleteReportAsync(id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Report>> ListReportsFilteredAsync(
            string? searchReportId,
            string? searchTitle,
            string? searchArea,
            DateTime? startDate,
            DateTime? endDate,
            int? categoryId,
            string? status)
        {
            // Use in-memory cache and BST for efficient filtering
            var reports = (await ListReportsAsync()).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchReportId))
            {
                if (Guid.TryParse(searchReportId, out var reportGuid))
                {
                    reports = reports.Where(r => r.Id == reportGuid);
                }
                else
                {
                    reports = reports.Where(r => r.Id.ToString().Contains(searchReportId));
                }
            }

            if (!string.IsNullOrWhiteSpace(searchTitle))
                reports = reports.Where(r => r.Description.Contains(searchTitle, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(searchArea))
            {
                var areaParts = searchArea.Split(',')
                    .Select(p => p.Trim())
                    .Where(p => !string.IsNullOrEmpty(p))
                    .ToList();

                reports = reports.Where(r =>
                    r.Address != null &&
                    areaParts.All(part =>
                        (!string.IsNullOrEmpty(r.Address.Street) && r.Address.Street.Contains(part, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(r.Address.Suburb) && r.Address.Suburb.Contains(part, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(r.Address.City) && r.Address.City.Contains(part, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(r.Address.Province) && r.Address.Province.Contains(part, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(r.Address.Country) && r.Address.Country.Contains(part, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(r.Address.FormattedAddress) && r.Address.FormattedAddress.Contains(part, StringComparison.OrdinalIgnoreCase))
                    )
                );
            }

            if (startDate.HasValue)
                reports = reports.Where(r => r.ReportedAt >= startDate.Value);
            if (endDate.HasValue)
                reports = reports.Where(r => r.ReportedAt <= endDate.Value);

            if (categoryId.HasValue)
                reports = reports.Where(r => r.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<IssueStatus>(status, out var parsedStatus))
                reports = reports.Where(r => r.Status == parsedStatus);

            // Use BST for date sorting if available
            if (_reportBst != null)
            {
                var filtered = _reportBst.InOrderTraversal()
                    .Select(w => w.Report)
                    .Where(r => reports.Any(x => x.Id == r.Id));
                return filtered;
            }

            return reports.OrderByDescending(r => r.ReportedAt).ToList();
        }

        /// <summary>
        /// Gets a list of status options for use in dropdowns, based on the <see cref="IssueStatus"/> enum.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="SelectListItem"/> representing status options.</returns>
        public IEnumerable<SelectListItem> GetIssueStatusSelectList()
        {
            return Enum.GetValues(typeof(IssueStatus))
                .Cast<IssueStatus>()
                .Select(e => new SelectListItem
                {
                    Value = e.ToString(),
                    Text = e == IssueStatus.InProgress ? "In Progress" : e.ToString()
                });
        }

        /// <summary>
        /// Builds a Binary Search Tree (BST) of reports keyed by ReportedAt.Ticks using a wrapper that implements IComparable.
        /// </summary>
        /// <param name="reports">The collection of reports.</param>
        /// <returns>A BST containing all reports, sorted by ReportedAt.Ticks.</returns>
        private BinarySearchTree<ReportByDateWrapper> BuildReportTree(IEnumerable<Report> reports)
        {
            var bst = new BinarySearchTree<ReportByDateWrapper>();
            foreach (var report in reports)
                bst.Insert(new ReportByDateWrapper(report));
            return bst;
        }

        /// <summary>
        /// [BST] Returns all reports sorted by ReportedAt using a Binary Search Tree (BST) for efficient O(log n) search and retrieval.
        /// </summary>
        /// <returns>Sorted list of reports.</returns>
        public async Task<IEnumerable<Report>> ListReportsSortedByDateAsync()
        {
            var reports = await _reportRepository.GetAllReportsAsync();
            var bst = BuildReportTree(reports);
            return bst.InOrderTraversal().Select(w => w.Report);
        }

        /// <summary>
        /// [BST] Searches reports in memory for those within the specified date range using a BST for O(log n) efficiency.
        /// </summary>
        /// <param name="start">Start of the date range (inclusive).</param>
        /// <param name="end">End of the date range (inclusive).</param>
        /// <returns>Reports within the date range.</returns>
        public async Task<IEnumerable<Report>> SearchReportsByDateRangeAsync(DateTime start, DateTime end)
        {
            var reports = await _reportRepository.GetAllReportsAsync();
            var bst = BuildReportTree(reports);
            return bst.InOrderTraversal()
                      .Select(w => w.Report)
                      .Where(r => r.ReportedAt >= start && r.ReportedAt <= end);
        }

        /// <summary>
        /// Builds a MinHeap of reports prioritized by Status and PriorityLevel.
        /// Only unresolved (e.g., Status == Reported) or high-priority requests are included.
        /// </summary>
        /// <param name="reports">The collection of reports.</param>
        /// <returns>A MinHeap containing prioritized reports.</returns>
        private MinHeap<ReportPriorityWrapper> BuildPriorityQueue(IEnumerable<Report> reports)
        {
            var heap = new MinHeap<ReportPriorityWrapper>();
            foreach (var report in reports)
            {
                // Example: treat IssueStatus.Reported as unresolved, and assume lower PriorityLevel means higher urgency
                if (report.Status == IssueStatus.Reported /* || report.PriorityLevel == PriorityLevel.High */)
                {
                    heap.Insert(new ReportPriorityWrapper(report));
                }
            }
            return heap;
        }

        /// <summary>
        /// Returns the top N most urgent unresolved reports using a MinHeap for prioritization.
        /// </summary>
        /// <param name="count">Number of urgent reports to return.</param>
        public async Task<IEnumerable<Report>> GetTopUrgentReportsAsync(int count = 5)
        {
            var reports = await _reportRepository.GetAllReportsAsync();
            var heap = BuildPriorityQueue(reports);
            var result = new List<Report>();
            for (int i = 0; i < count && heap.Count > 0; i++)
            {
                result.Add(heap.ExtractMin().Report);
            }
            return result;
        }

        /// <summary>
        /// Returns the top N most recent reports using a MaxHeap keyed by ReportedAt (most recent first).
        /// This is more efficient than full list sorting for dashboard "Recent Reports".
        /// </summary>
        /// <param name="count">Number of recent reports to return (default 10).</param>
        /// <returns>List of the most recent reports.</returns>
        public async Task<IEnumerable<Report>> GetTopRecentReportsAsync(int count = 10)
        {
            var reports = await _reportRepository.GetAllReportsAsync();
            var heap = new MaxHeap<Report>(Comparer<Report>.Create(
                (a, b) => b.ReportedAt.CompareTo(a.ReportedAt) // MaxHeap: most recent first
            ));
            foreach (var report in reports)
                heap.Insert(report);

            var result = new List<Report>();
            for (int i = 0; i < count && heap.Count > 0; i++)
            {
                result.Add(heap.ExtractMax());
            }
            return result;
        }

        /// <summary>
        /// Generic max-heap data structure for use in dashboard queries.
        /// </summary>
        /// <typeparam name="T">The type of elements stored, must implement IComparable&lt;T&gt; or use a custom comparer.</typeparam>
        public class MaxHeap<T>
        {
            private readonly List<T> _elements = new();
            private readonly Comparer<T> _comparer;

            public MaxHeap(Comparer<T>? comparer = null)
            {
                _comparer = comparer ?? Comparer<T>.Default;
            }

            public int Count => _elements.Count;

            public void Insert(T value)
            {
                _elements.Add(value);
                HeapifyUp(_elements.Count - 1);
            }

            public T ExtractMax()
            {
                if (_elements.Count == 0)
                    throw new InvalidOperationException("Heap is empty.");
                T max = _elements[0];
                _elements[0] = _elements[^1];
                _elements.RemoveAt(_elements.Count - 1);
                HeapifyDown(0);
                return max;
            }

            private void HeapifyUp(int index)
            {
                while (index > 0)
                {
                    int parent = (index - 1) / 2;
                    if (_comparer.Compare(_elements[index], _elements[parent]) <= 0)
                        break;
                    (_elements[index], _elements[parent]) = (_elements[parent], _elements[index]);
                    index = parent;
                }
            }

            private void HeapifyDown(int index)
            {
                int lastIndex = _elements.Count - 1;
                while (true)
                {
                    int left = 2 * index + 1;
                    int right = 2 * index + 2;
                    int largest = index;

                    if (left <= lastIndex && _comparer.Compare(_elements[left], _elements[largest]) > 0)
                        largest = left;
                    if (right <= lastIndex && _comparer.Compare(_elements[right], _elements[largest]) > 0)
                        largest = right;

                    if (largest == index)
                        break;

                    (_elements[index], _elements[largest]) = (_elements[largest], _elements[index]);
                    index = largest;
                }
            }
        }



        /// <summary>
        /// Demonstrates graph traversal by finding related service requests using BFS from the given root request.
        /// Builds a graph where each node is a report, and edges connect reports with the same category or suburb.
        /// Optionally computes a Minimum Spanning Tree (MST) to visualize geographically close reports.
        /// </summary>
        /// <param name="rootId">The root report ID to start traversal from.</param>
        /// <returns>All related reports discovered via BFS traversal.</returns>
        public async Task<IEnumerable<Report>> GetRelatedRequestsByGraphAsync(Guid rootId)
        {
            // Fetch all reports
            var allReports = (await ListReportsAsync()).ToList();
            var reportDict = allReports.ToDictionary(r => r.Id, r => r);

            // Build a graph where each node is a report, and edges connect reports with the same category or suburb
            var graph = new Graph<Guid>();

            // Add vertices
            foreach (var report in allReports)
                graph.AddVertex(report.Id);

            // Add edges: connect reports with the same CategoryId
            var categoryGroups = allReports.GroupBy(r => r.CategoryId);
            foreach (var group in categoryGroups)
            {
                var ids = group.Select(r => r.Id).ToList();
                for (int i = 0; i < ids.Count; i++)
                    for (int j = i + 1; j < ids.Count; j++)
                        graph.AddEdge(ids[i], ids[j]);
            }

            // Add edges: connect reports with the same Suburb (case-insensitive, non-empty)
            var suburbGroups = allReports
                .Where(r => r.Address?.Suburb != null && !string.IsNullOrWhiteSpace(r.Address.Suburb))
                .GroupBy(r => r.Address!.Suburb!.Trim().ToLowerInvariant());
            foreach (var group in suburbGroups)
            {
                var ids = group.Select(r => r.Id).ToList();
                for (int i = 0; i < ids.Count; i++)
                    for (int j = i + 1; j < ids.Count; j++)
                        graph.AddEdge(ids[i], ids[j]);
            }

            // Traverse from rootId using BFS to get all related reports
            var relatedIds = graph.Bfs(rootId).ToList();
            var relatedReports = relatedIds.Select(id => reportDict[id]);

            // Only include reports with valid latitude/longitude
            var geoReports = relatedReports
                .Where(r => r.Address?.Latitude != null && r.Address?.Longitude != null)
                .ToList();

            if (geoReports.Count > 1)
            {
                var geoGraph = new Graph<Guid>();
                foreach (var report in geoReports)
                    geoGraph.AddVertex(report.Id);

                for (int i = 0; i < geoReports.Count; i++)
                {
                    for (int j = i + 1; j < geoReports.Count; j++)
                    {
                        var r1 = geoReports[i];
                        var r2 = geoReports[j];
                        double distance = HaversineDistance(
                            r1.Address!.Latitude!.Value, r1.Address.Longitude!.Value,
                            r2.Address!.Latitude!.Value, r2.Address.Longitude!.Value);
                        geoGraph.AddEdge(r1.Id, r2.Id, distance);
                    }
                }

                var mstEdges = geoGraph.GetMinimumSpanningTree();
            }

            return relatedReports;
        }

        /// <summary>
        /// Calculates the Haversine distance (in kilometers) between two latitude/longitude points.
        /// </summary>
        private static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth radius in km
            double dLat = DegreesToRadians(lat2 - lat1);
            double dLon = DegreesToRadians(lon2 - lon1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double DegreesToRadians(double deg) => deg * (Math.PI / 180);
    }

    /// <summary>
    /// Wrapper class to enable MinHeap sorting for Report by PriorityLevel.
    /// Lower PriorityLevel means higher urgency.
    /// </summary>
    public class ReportPriorityWrapper : IComparable<ReportPriorityWrapper>
    {
        public Report Report { get; }
        public int PriorityLevel { get; }

        public ReportPriorityWrapper(Report report)
        {
            Report = report;
            PriorityLevel = (int)(report.GetType().GetProperty("PriorityLevel")?.GetValue(report) ?? 0);
        }

        public int CompareTo(ReportPriorityWrapper other)
        {
            if (object.ReferenceEquals(other, null)) return -1;
            return PriorityLevel.CompareTo(other.PriorityLevel);
        }
    }

    /// <summary>
    /// Wrapper class to enable BST sorting for Report by ReportedAt.Ticks.
    /// </summary>
    public class ReportByDateWrapper : IComparable<ReportByDateWrapper>
    {
        public Report Report { get; }
        public ReportByDateWrapper(Report report) => Report = report;
        public int CompareTo(ReportByDateWrapper other)
        {
            if (object.ReferenceEquals(other, null)) return 1;
            return Report.ReportedAt.Ticks.CompareTo(other.Report.ReportedAt.Ticks);
        }
    }

    /// <summary>
    /// Wrapper class to enable BST sorting for Report by ReportedAt (legacy, not used in new BST logic).
    /// </summary>
    public class ReportWrapper : IComparable<ReportWrapper>
    {
        public Report Report { get; }
        public ReportWrapper(Report report) => Report = report;
        public int CompareTo(ReportWrapper other)
        {
            if (object.ReferenceEquals(other, null)) return 1;
            return Report.ReportedAt.CompareTo(other.Report.ReportedAt);
        }
    }
}
