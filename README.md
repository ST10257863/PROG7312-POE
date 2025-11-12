# PROG7312POE

## Overview
This application is a municipal issue reporting system built with ASP.NET Core Razor Pages. It enables residents to report local issues (such as roads, water, electricity, sanitation, etc.), attach files (images or documents), and receive confirmation and status updates. The user interface is designed to be clear, consistent, and user-friendly, with feedback and engagement features to encourage participation.

## Features
- **Main Menu:** On startup, users see three options: Report Issues (enabled), Local Events and Announcements (enabled), and Service Request Status (disabled).
- **Report Issues:** Users can provide location, select a category, describe the issue, and attach images or documents. The interface includes feedback and engagement elements.
- **Attachments:** Users can upload images or documents related to their report.
- **Google Maps Places Autocomplete:** The address field in the report form uses Google Maps Places Autocomplete to help users quickly and accurately enter their location. Suggestions are restricted to geocoded locations within South Africa.

## Integration of Data Structures

### Summary of Implemented Structures

- **Binary Search Tree (BST):**
  - Used to index and retrieve service requests sorted by their creation date.
  - Enables efficient O(log n) search and in-order traversal for sorted display.
  - **Example call site:**  
    `GET /Reports/TreeView` in `ReportController` calls `ListReportsSortedByDateAsync()` in `ReportService`.

- **MinHeap (Priority Queue):**
  - Used to prioritize unresolved or urgent service requests by their priority level.
  - Supports O(log n) insertion and extraction of the most urgent request.
  - **Example call site:**  
    `GET /Reports/PriorityQueue` in `ReportController` calls `GetTopUrgentReportsAsync()` in `ReportService`.

- **Graph (Adjacency List):**
  - Used to model relationships between service requests (e.g., by category).
  - Enables efficient traversal (BFS/DFS) to find related requests.
  - **Example call site:**  
    `GET /Reports/GraphView/{id}` in `ReportController` calls `GetRelatedRequestsByGraphAsync()` in `ReportService`.

### Role in Service Request Status Feature

- **Tree:**  
  Provides fast, sorted access to service requests, allowing users and staff to view requests in chronological order or search by date range efficiently.

- **Heap:**  
  Ensures that the most urgent or unresolved requests are surfaced quickly, supporting escalation and prioritization workflows.

- **Graph:**  
  Allows visualization and discovery of related or dependent requests, such as issues in the same category or area, supporting better decision-making and resource allocation.

### Time Complexity Improvements

- **Binary Search Tree:**  
  - Lookup, insertion, and deletion: O(log n) (on average, for balanced trees)
  - In-order traversal: O(n)
- **MinHeap:**  
  - Insertion: O(log n)
  - ExtractMin (get most urgent): O(log n)
- **Graph (BFS/DFS):**  
  - Traversal: O(V + E), where V = number of vertices (requests), E = number of edges (relationships)

---

## Data Storage Implementation

### Database Storage with Entity Framework Core
The application uses a database-backed repository for data persistence. All reports, attachments, categories, and users are stored in a SQL database via Entity Framework Core (`AppDbContext`). This ensures that data survives application restarts and supports scalability.

- **Persistence:** Data is saved in a relational database and remains available after restarts.
- **EF Core:** The repository implementation uses `AppDbContext` for all CRUD operations.
- **Attachments:** Uploaded files are stored in the database and associated with their reports.
- **Production-Ready:** The application is suitable for real-world use with robust data storage.

### Database Setup: Using LocalDB

To use the database-backed storage with LocalDB, follow these steps:

1. **Ensure LocalDB is Installed**
   - LocalDB is included with Visual Studio. If you have Visual Studio installed, you likely already have LocalDB.
   - To verify, open a command prompt and run:
     ```
     sqllocaldb info
     ```
   - If not installed, you can add it via the Visual Studio Installer (select the "Data storage and processing" workload).

2. **Configure the Connection String**
   - Open `appsettings.json` and ensure the connection string for `ApplicationDbConnection` is set to use LocalDB. Example:
     ```json
     "ConnectionStrings": {
       "ApplicationDbConnection": "Server=(localdb)\\mssqllocaldb;Database=MunicipalityDb;Trusted_Connection=True;MultipleActiveResultSets=true"
     }
     ```
   - You may change `Database=MunicipalityDb` to your preferred database name.

3. **Apply Entity Framework Migrations**
   - Open the Package Manager Console in Visual Studio (__Tools > NuGet Package Manager > Package Manager Console__).
   - Run the following commands to create and apply the database schema:
     ```
     Update-Database
     ```
   - If you have not created migrations yet, run:
     ```
     Add-Migration InitialCreate
     Update-Database
     ```

4. **Run the Application**
   - Build and run the project. The database will be created automatically if it does not exist.

**Troubleshooting:**
- If you encounter connection issues, ensure LocalDB is running and the connection string matches your environment.
- You can start LocalDB manually with:
    ```
    sqllocaldb start 
    sqllocaldb create
    ```
- 
**Summary:**  
LocalDB is a lightweight SQL Server instance for development. With the correct connection string and migrations applied, your application will use LocalDB for persistent storage.

## How to Run
1. Clone the repository.
2. Open the solution in Visual Studio.
3. Build and run the project.
4. Use the web interface to report issues and attach files.

### Google Maps API Key
To enable the address autocomplete feature, you must provide a Google Maps API key. Set the key in your configuration (e.g., `appsettings.json` or as an environment variable) and ensure it is available to the view via `ViewBag.GoogleMapsApiKey`.

---

**Note:** All data is persistent when using the database option.

---

## Implementation Report

### Why These Structures Were Chosen

- **Tree (BST):**  
  Enables fast, sorted access to service requests by date, supporting efficient range queries and chronological views.
- **Heap (MinHeap):**  
  Allows urgent or unresolved requests to be prioritized and surfaced quickly, which is essential for escalation and response workflows.
- **Graph:**  
  Models relationships between requests (e.g., by category or area), enabling discovery of dependencies and related issues for better resource allocation.

### How They Improved Efficiency and Maintainability

- **Efficiency:**  
  - Tree and heap structures reduce the time complexity of common operations (search, sort, prioritization) from O(n log n) or O(n) to O(log n) for insertion and retrieval.
  - Graph traversal enables O(V + E) discovery of related requests, which is much faster than repeated linear scans.
- **Maintainability:**  
  - Encapsulating these algorithms in reusable classes (BST, MinHeap, Graph) keeps the codebase modular and testable.
  - Service and controller methods clearly indicate where each structure is used, making the code easy to understand and extend.

### Example Usage

- **Heap Prioritizing Urgent Requests:**  
  The `GetTopUrgentReportsAsync` method in `ReportService` uses a MinHeap to extract the top 5 most urgent unresolved requests.  
  Example endpoint: `GET /Reports/PriorityQueue`

- **Graph Showing Related Service Dependencies:**  
  The `GetRelatedRequestsByGraphAsync` method builds a graph of reports connected by category and uses BFS to find all related requests.  
  Example endpoint: `GET /Reports/GraphView/{id}`

- **Tree for Sorted Results:**  
  The `ListReportsSortedByDateAsync` method uses a BST to return all reports sorted by creation date.  
  Example endpoint: `GET /Reports/TreeView`
