using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Municipality_Application.Interfaces;
using Municipality_Application.Interfaces.Service;
using Municipality_Application.Models;
using Municipality_Application.Services.DataStructures;

namespace Municipality_Application.Services
{
    /// <summary>
    /// Provides methods for managing and retrieving report data, including advanced data structure integrations for efficient access and filtering.
    /// </summary>
    public class ReportService : IReportService
    {
        #region Fields

        private readonly IReportRepository _reportRepository;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<ReportService> _logger;

        private BinarySearchTree<ReportByDateWrapper>? _reportBst;
        private MaxHeap<Report>? _recentHeap;
        private MinHeap<ReportPriorityWrapper>? _priorityHeap;

        private const string CacheKey = "RecentReportsCache";
        private const int MaxReports = 2000;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportService"/> class.
        /// </summary>
        /// <param name="reportRepository">The report repository.</param>
        /// <param name="memoryCache">The memory cache instance.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="appLifetime">The application lifetime instance.</param>
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

        #endregion

        #region Public Methods (IReportService Implementation)

        /// <inheritdoc/>
        public async Task<IEnumerable<Report>> ListReportsAsync(bool forceRefresh = false)
        {
            // Try to get reports from cache unless forceRefresh is requested
            if (!forceRefresh && _memoryCache.TryGetValue(CacheKey, out List<Report> cachedReports))
            {
                // Always rebuild in-memory structures from cache
                BuildInMemoryStructures(cachedReports);
                return cachedReports;
            }

            // Fetch all reports from the repository, order by date, and take the most recent
            var allReports = (await _reportRepository.GetAllReportsAsync())
                .OrderByDescending(r => r.ReportedAt)
                .Take(MaxReports)
                .ToList();

            // Build in-memory structures for fast access and cache the result
            BuildInMemoryStructures(allReports);
            _memoryCache.Set(CacheKey, allReports, CacheDuration);
            return allReports;
        }

        /// <inheritdoc/>
        public async Task<Report> SubmitReportAsync(Report report, List<IFormFile> files)
        {
            // Ensure the report has a unique ID
            if (report.Id == Guid.Empty)
            {
                report.Id = Guid.NewGuid();
            }

            // Process and attach any uploaded files
            report.Attachments = await ProcessAttachmentsAsync(report.Id, files);
            // Save the report to the repository
            return await _reportRepository.AddReportAsync(report);
        }

        /// <inheritdoc/>
        public async Task<Report?> GetReportDetailsAsync(Guid id)
        {
            // Retrieve a single report by its ID
            return await _reportRepository.GetReportByIdAsync(id);
        }

        /// <inheritdoc/>
        public async Task<bool> ModifyReportAsync(Report report)
        {
            // Update an existing report
            return await _reportRepository.UpdateReportAsync(report);
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveReportAsync(Guid id)
        {
            // Delete a report by its ID
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
            // Start with all reports (from cache or DB)
            var reports = (await ListReportsAsync()).AsQueryable();

            // Filter by report ID (exact or partial match)
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

            // Filter by title/description
            if (!string.IsNullOrWhiteSpace(searchTitle))
                reports = reports.Where(r => r.Description.Contains(searchTitle, StringComparison.OrdinalIgnoreCase));

            // Filter by area (street, suburb, city, etc.)
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

            // Filter by date range
            if (startDate.HasValue)
                reports = reports.Where(r => r.ReportedAt >= startDate.Value);
            if (endDate.HasValue)
                reports = reports.Where(r => r.ReportedAt <= endDate.Value);

            // Filter by category
            if (categoryId.HasValue)
                reports = reports.Where(r => r.CategoryId == categoryId.Value);

            // Filter by status
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<IssueStatus>(status, out var parsedStatus))
                reports = reports.Where(r => r.Status == parsedStatus);

            // If BST is available, use it for efficient date sorting
            if (_reportBst != null)
            {
                var filtered = _reportBst.InOrderTraversal()
                    .Select(w => w.Report)
                    .Where(r => reports.Any(x => x.Id == r.Id));
                return filtered;
            }

            // Otherwise, just order by date
            return reports.OrderByDescending(r => r.ReportedAt).ToList();
        }

        /// <inheritdoc/>
        public IEnumerable<SelectListItem> GetIssueStatusSelectList()
        {
            // Build a dropdown list from the IssueStatus enum
            return Enum.GetValues(typeof(IssueStatus))
                .Cast<IssueStatus>()
                .Select(e => new SelectListItem
                {
                    Value = e.ToString(),
                    Text = e == IssueStatus.InProgress ? "In Progress" : e.ToString()
                });
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Report>> ListReportsSortedByDateAsync()
        {
            // Get all reports and build a BST for date sorting
            var reports = await _reportRepository.GetAllReportsAsync();
            var bst = BuildReportTree(reports);
            return bst.InOrderTraversal().Select(w => w.Report);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Report>> SearchReportsByDateRangeAsync(DateTime start, DateTime end)
        {
            // Get all reports and use BST to efficiently find those in the date range
            var reports = await _reportRepository.GetAllReportsAsync();
            var bst = BuildReportTree(reports);
            return bst.InOrderTraversal()
                      .Select(w => w.Report)
                      .Where(r => r.ReportedAt >= start && r.ReportedAt <= end);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Report>> GetTopUrgentReportsAsync(int count = 5)
        {
            // Get all reports and build a MinHeap for urgent (unresolved) reports
            var reports = await _reportRepository.GetAllReportsAsync();
            var heap = BuildPriorityQueue(reports);
            var result = new List<Report>();
            // Extract the top N urgent reports
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
            // Get all reports and build a MaxHeap for most recent reports
            var reports = await _reportRepository.GetAllReportsAsync();
            var heap = new MaxHeap<Report>(Comparer<Report>.Create(
                (a, b) => b.ReportedAt.CompareTo(a.ReportedAt)
            ));
            foreach (var report in reports)
                heap.Insert(report);

            var result = new List<Report>();
            // Extract the top N most recent reports
            for (int i = 0; i < count && heap.Count > 0; i++)
            {
                result.Add(heap.ExtractMax());
            }
            return result;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Report>> GetRelatedRequestsByGraphAsync(Guid rootId)
        {
            // Get all reports and build a graph where nodes are reports and edges connect related reports
            var allReports = (await ListReportsAsync()).ToList();
            var reportDict = allReports.ToDictionary(r => r.Id, r => r);

            var graph = new Graph<Guid>();

            // Add all reports as vertices
            foreach (var report in allReports)
                graph.AddVertex(report.Id);

            // Connect reports with the same category
            var categoryGroups = allReports.GroupBy(r => r.CategoryId);
            foreach (var group in categoryGroups)
            {
                var ids = group.Select(r => r.Id).ToList();
                for (int i = 0; i < ids.Count; i++)
                    for (int j = i + 1; j < ids.Count; j++)
                        graph.AddEdge(ids[i], ids[j]);
            }

            // Connect reports with the same suburb
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

            // Use BFS to find all related reports from the root
            var relatedIds = graph.Bfs(rootId).ToList();
            var relatedReports = relatedIds.Select(id => reportDict[id]);

            // If there are enough geo-located reports, build a geo-graph for MST (visualization)
            var geoReports = relatedReports
                .Where(r => r.Address?.Latitude != null && r.Address?.Longitude != null)
                .ToList();

            if (geoReports.Count > 1)
            {
                var geoGraph = new Graph<Guid>();
                foreach (var report in geoReports)
                    geoGraph.AddVertex(report.Id);

                // Add weighted edges based on Haversine distance
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

                // Optionally, get the minimum spanning tree (not used in return value)
                var mstEdges = geoGraph.GetMinimumSpanningTree();
            }

            return relatedReports;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Builds in-memory data structures (BST, MaxHeap, MinHeap) for fast access and filtering.
        /// </summary>
        /// <param name="reports">The list of reports to build structures from.</param>
        private void BuildInMemoryStructures(List<Report> reports)
        {
            // Build a BST for date-based queries
            _reportBst = new BinarySearchTree<ReportByDateWrapper>();
            foreach (var report in reports)
                _reportBst.Insert(new ReportByDateWrapper(report));

            // Build a MaxHeap for recent reports
            _recentHeap = new MaxHeap<Report>(Comparer<Report>.Create(
                (a, b) => b.ReportedAt.CompareTo(a.ReportedAt)
            ));
            foreach (var report in reports)
                _recentHeap.Insert(report);

            // Build a MinHeap for urgent (unresolved) reports
            _priorityHeap = new MinHeap<ReportPriorityWrapper>();
            foreach (var report in reports)
            {
                if (report.Status == IssueStatus.Reported)
                    _priorityHeap.Insert(new ReportPriorityWrapper(report));
            }
        }

        /// <summary>
        /// Processes file attachments for a report, enforcing a 5MB size limit and converting files to base64 data URLs.
        /// </summary>
        /// <param name="reportId">The report ID to associate attachments with.</param>
        /// <param name="files">The list of files to process.</param>
        /// <returns>A list of <see cref="Attachment"/> objects.</returns>
        private async Task<List<Attachment>> ProcessAttachmentsAsync(Guid reportId, List<IFormFile> files)
        {
            const long MaxFileSize = 5 * 1024 * 1024;
            var attachments = new List<Attachment>();

            // Loop through each file and process if valid
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        // Enforce file size limit
                        if (file.Length > MaxFileSize)
                        {
                            throw new Exception($"File '{file.FileName}' exceeds the 5MB size limit.");
                        }

                        // Read file into memory and convert to base64 data URL
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
        /// Builds a Binary Search Tree (BST) of reports keyed by ReportedAt.Ticks using a wrapper that implements IComparable.
        /// </summary>
        /// <param name="reports">The collection of reports.</param>
        /// <returns>A BST containing all reports, sorted by ReportedAt.Ticks.</returns>
        private BinarySearchTree<ReportByDateWrapper> BuildReportTree(IEnumerable<Report> reports)
        {
            // Insert each report into the BST using a date wrapper
            var bst = new BinarySearchTree<ReportByDateWrapper>();
            foreach (var report in reports)
                bst.Insert(new ReportByDateWrapper(report));
            return bst;
        }

        /// <summary>
        /// Builds a MinHeap of reports prioritized by Status and PriorityLevel.
        /// Only unresolved (e.g., Status == Reported) or high-priority requests are included.
        /// </summary>
        /// <param name="reports">The collection of reports.</param>
        /// <returns>A MinHeap containing prioritized reports.</returns>
        private MinHeap<ReportPriorityWrapper> BuildPriorityQueue(IEnumerable<Report> reports)
        {
            // Insert unresolved reports into the MinHeap
            var heap = new MinHeap<ReportPriorityWrapper>();
            foreach (var report in reports)
            {
                if (report.Status == IssueStatus.Reported)
                {
                    heap.Insert(new ReportPriorityWrapper(report));
                }
            }
            return heap;
        }

        /// <summary>
        /// Calculates the Haversine distance (in kilometers) between two latitude/longitude points.
        /// </summary>
        /// <param name="lat1">Latitude of the first point.</param>
        /// <param name="lon1">Longitude of the first point.</param>
        /// <param name="lat2">Latitude of the second point.</param>
        /// <param name="lon2">Longitude of the second point.</param>
        /// <returns>The distance in kilometers.</returns>
        private static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // Standard Haversine formula for distance on a sphere
            const double R = 6371;
            double dLat = DegreesToRadians(lat2 - lat1);
            double dLon = DegreesToRadians(lon2 - lon1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        /// <param name="deg">Degrees value.</param>
        /// <returns>Radians value.</returns>
        private static double DegreesToRadians(double deg) => deg * (Math.PI / 180);

        #endregion

        #region Nested Types

        /// <summary>
        /// Generic max-heap data structure for use in dashboard queries.
        /// </summary>
        /// <typeparam name="T">The type of elements stored, must implement IComparable&lt;T&gt; or use a custom comparer.</typeparam>
        public class MaxHeap<T>
        {
            private readonly List<T> _elements = new();
            private readonly Comparer<T> _comparer;

            /// <summary>
            /// Initializes a new instance of the <see cref="MaxHeap{T}"/> class.
            /// </summary>
            /// <param name="comparer">Optional custom comparer for heap ordering.</param>
            public MaxHeap(Comparer<T>? comparer = null)
            {
                _comparer = comparer ?? Comparer<T>.Default;
            }

            /// <summary>
            /// Gets the number of elements in the heap.
            /// </summary>
            public int Count => _elements.Count;

            /// <summary>
            /// Inserts a value into the max-heap.
            /// </summary>
            /// <param name="value">The value to insert.</param>
            public void Insert(T value)
            {
                _elements.Add(value);
                HeapifyUp(_elements.Count - 1);
            }

            /// <summary>
            /// Extracts and returns the maximum element from the heap.
            /// </summary>
            /// <returns>The maximum element.</returns>
            /// <exception cref="InvalidOperationException">Thrown if the heap is empty.</exception>
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
                // Move the new element up to maintain heap property
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
                // Move the root element down to maintain heap property
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
        /// Wrapper class to enable MinHeap sorting for Report by PriorityLevel.
        /// Lower PriorityLevel means higher urgency.
        /// </summary>
        public class ReportPriorityWrapper : IComparable<ReportPriorityWrapper>
        {
            /// <summary>
            /// Gets the wrapped report.
            /// </summary>
            public Report Report { get; }
            /// <summary>
            /// Gets the priority level of the report.
            /// </summary>
            public int PriorityLevel { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="ReportPriorityWrapper"/> class.
            /// </summary>
            /// <param name="report">The report to wrap.</param>
            public ReportPriorityWrapper(Report report)
            {
                Report = report;
                PriorityLevel = (int)(report.GetType().GetProperty("PriorityLevel")?.GetValue(report) ?? 0);
            }

            /// <inheritdoc/>
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
            /// <summary>
            /// Gets the wrapped report.
            /// </summary>
            public Report Report { get; }
            /// <summary>
            /// Initializes a new instance of the <see cref="ReportByDateWrapper"/> class.
            /// </summary>
            /// <param name="report">The report to wrap.</param>
            public ReportByDateWrapper(Report report) => Report = report;
            /// <inheritdoc/>
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
            /// <summary>
            /// Gets the wrapped report.
            /// </summary>
            public Report Report { get; }
            /// <summary>
            /// Initializes a new instance of the <see cref="ReportWrapper"/> class.
            /// </summary>
            /// <param name="report">The report to wrap.</param>
            public ReportWrapper(Report report) => Report = report;
            /// <inheritdoc/>
            public int CompareTo(ReportWrapper other)
            {
                if (object.ReferenceEquals(other, null)) return 1;
                return Report.ReportedAt.CompareTo(other.Report.ReportedAt);
            }
        }

        #endregion
    }
}