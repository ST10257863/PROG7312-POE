# PROG7312POE

## Overview

This application is a municipal issue reporting system built with ASP.NET Core Razor Pages. It enables residents to report local issues (such as roads, water, electricity, sanitation, etc.), attach files (images or documents), and receive confirmation and status updates. The user interface is designed to be clear, consistent, and user-friendly, with feedback and engagement features to encourage participation.

---

## How to Compile and Run

### Prerequisites

- **.NET 8 SDK** (Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download))
- **Visual Studio 2022** (recommended, with ASP.NET and web development workload)
- **SQL Server LocalDB** (comes with Visual Studio)
- **Google Maps API Key** (for address autocomplete)

### Steps

1. **Clone the Repository**
```
git clone https://github.com/VCCT-PROG7312-2025-G3/PROG7312-POE-ST10257863.git 
cd PROG7312-POE-ST10257863
```

2. **Open the Solution**
- Open the `.sln` file in Visual Studio 2022.

3. **Configure the Database**
- Open `appsettings.json`.
- Ensure the connection string for `ApplicationDbConnection` uses LocalDB:
  ```json
  "ConnectionStrings": {
    "ApplicationDbConnection": "Server=(localdb)\\mssqllocaldb;Database=MunicipalityDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
  ```
- You can change the database name if you wish.

4. **Apply Entity Framework Migrations**
- In Visual Studio, open __Tools > NuGet Package Manager > Package Manager Console__.
- Run:
  ```
  Update-Database
  ```
- If you haven't created migrations yet, run:
  ```
  Add-Migration InitialCreate
  Update-Database
  ```

5. **Set Up Google Maps API Key**
- Get an API key from [Google Cloud Console](https://console.cloud.google.com/).
- Add it to your `appsettings.json` or as an environment variable.
- The key is used for address autocomplete in the report form.

6. **Build and Run**
- Press F5 or click the "Run" button in Visual Studio.
- The app will open in your browser.

---

## How to Use

- **Report Issues:**  
Go to the main page, select "Report Issues", fill in the form (location, category, description, attachments), and submit.
- **View Status:**  
Use the "Service Request Status" feature to search, filter, and view the status of submitted reports.
- **Advanced Views:**  
- **Tree View:** See all reports sorted by date.
- **Priority Queue:** See the most urgent unresolved requests.
- **Graph View:** Visualize related requests by category or area.

---

## Data Structures in "Service Request Status"

The "Service Request Status" feature is powered by three core data structures: **Binary Search Tree (BST)**, **MinHeap (Priority Queue)**, and **Graph**. Each structure is implemented in C# and used in the `ReportService` and exposed via endpoints in `ReportController`.

### 1. Binary Search Tree (BST)

**What it does:**  
The BST is used to keep reports sorted by their creation date (`ReportedAt`). This allows the system to quickly retrieve all reports in chronological order or search for reports within a specific date range.

**How it works:**  
- When reports are loaded (see `ReportService.ListReportsAsync`), they are inserted into a BST (`BinarySearchTree<ReportByDateWrapper>`).
- The BST supports fast in-order traversal, so you can get all reports sorted by date in O(n) time, and insertions/searches are O(log n) on average.

**Where it's used:**  
- `ReportController.TreeView` calls `ReportService.ListReportsSortedByDateAsync()`, which returns all reports sorted by date using the BST.
- `ReportService.SearchReportsByDateRangeAsync(start, end)` uses the BST to efficiently find reports within a date range.

**Why it's efficient:**  
- Instead of sorting the entire list every time, the BST keeps things sorted as you go.
- This is especially useful when you have a lot of reports and need to support fast date-based queries.

**Example:**
~~~
{
// In ReportService 
public async Task<IEnumerable<Report>> ListReportsSortedByDateAsync() 
{ 
    var reports = await _reportRepository.GetAllReportsAsync(); 
    var bst = BuildReportTree(reports); 

    return bst.InOrderTraversal().Select(w => w.Report); 
}
~~~

---

### 2. MinHeap (Priority Queue)

**What it does:**  
The MinHeap is used to prioritize unresolved or urgent service requests. Each report is wrapped with a priority (e.g., severity, status), and the heap always keeps the most urgent request at the top.

**How it works:**  
- When reports are loaded, unresolved ones (e.g., `Status == Reported`) are inserted into a `MinHeap<ReportPriorityWrapper>`.
- The heap allows O(log n) insertion and extraction of the most urgent report.

**Where it's used:**  
- `ReportController.PriorityQueue` calls `ReportService.GetTopUrgentReportsAsync(count)`, which extracts the top N urgent reports from the MinHeap.

**Why it's efficient:**  
- Instead of scanning the whole list to find the most urgent requests, the MinHeap always has the most urgent at the top, so you can get it in O(log n) time.
- This is great for dashboards or admin panels where you want to see what needs attention right now.

**Example:**
~~~
// In ReportService 
public async Task<IEnumerable<Report>> GetTopUrgentReportsAsync(int count = 5) 
{ 
    // Get all reports and build a MinHeap for urgent (unresolved) reports
    var reports = await _reportRepository.GetAllReportsAsync(); 
    var heap = new MinHeap<ReportPriorityWrapper>(); 
    foreach (var report in reports) 
    { 
        if (report.Status == IssueStatus.Reported) 
        { 
            heap.Insert(new ReportPriorityWrapper(report)); 
        } 
    } 
    var result = new List<Report>(); 

    // Extract the top N urgent reports 
    for (int i = 0; i < count && heap.Count > 0; i++) 
    { 
        result.Add(heap.ExtractMin().Report); 
    } 

    return result; 
}
~~~
---

### 3. Graph (Adjacency List)

**What it does:**  
The Graph models relationships between reports, such as those in the same category or suburb. This allows the system to find and visualize related requests, which is useful for identifying clusters of issues or dependencies.

**How it works:**  
- Each report is a node in the graph.
- Edges are added between reports that share the same category or suburb.
- Breadth-First Search (BFS) is used to find all reports related to a given report.

**Where it's used:**  
- `ReportController.GraphView` calls `ReportService.GetRelatedRequestsByGraphAsync(id)`, which returns all related reports using BFS traversal.

**Why it's efficient:**  
- Instead of repeatedly scanning the list for related reports, the graph structure allows you to traverse all related nodes in O(V + E) time (where V = number of reports, E = number of relationships).
- This is especially useful for visualizations and for understanding how issues might be connected.

**Example:**
~~~
// In ReportService 
public async Task<IEnumerable<Report>> GetRelatedRequestsByGraphAsync(Guid rootId) 
{ 
    var allReports = (await ListReportsAsync()).ToList(); 
    var reportDict = allReports.ToDictionary(r => r.Id, r => r);
    var graph = new Graph<Guid>();

    // ... build graph by category and suburb ...
    var relatedIds = graph.Bfs(rootId).ToList();

    return relatedIds.Select(id => reportDict[id]);
}
~~~

---

## Data Storage

- **Entity Framework Core** is used for all data persistence.
- All reports, attachments, categories, and users are stored in a SQL database.
- Attachments are stored in the database as base64-encoded data URLs.
- The repository pattern (`EfReportRepository`) abstracts all database operations.

---

## Troubleshooting

- **Database connection issues:**  
  - Make sure LocalDB is installed and running.
  - Check your connection string in `appsettings.json`.
  - Use the Package Manager Console to apply migrations.

- **Google Maps API issues:**  
  - Make sure your API key is valid and has the right permissions.
  - Check the browser console for errors if autocomplete isn't working.

---

## Summary

This project demonstrates how classic data structures (BST, MinHeap, Graph) can be used in a real-world web application to make searching, sorting, prioritizing, and visualizing data much more efficient. The code is modular, well-documented, and designed for both maintainability and performance.

If you want to see these structures in action, try the "Service Request Status" features in the app—especially the Tree View, Priority Queue, and Graph View!