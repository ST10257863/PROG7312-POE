# PROG7312POE

## Overview
This application is a municipal issue reporting system built with ASP.NET Core Razor Pages. It enables residents to report local issues (such as roads, water, electricity, sanitation, etc.), attach files (images or documents), and receive confirmation and status updates. The user interface is designed to be clear, consistent, and user-friendly, with feedback and engagement features to encourage participation.

## Features
- **Main Menu:** On startup, users see three options: Report Issues (enabled), Local Events and Announcements (disabled), and Service Request Status (disabled).
- **Report Issues:** Users can provide location, select a category, describe the issue, and attach images or documents. The interface includes feedback and engagement elements.
- **Attachments:** Users can upload images or documents related to their report.
- **Google Maps Places Autocomplete:** The address field in the report form uses Google Maps Places Autocomplete to help users quickly and accurately enter their location. Suggestions are restricted to geocoded locations within South Africa.

## Data Storage Implementation

### Switching Between In-Memory and Database Storage
You can easily switch between in-memory and database-backed storage by commenting or uncommenting specific lines in [`Program.cs`] see lines 15-23 in [`Program.cs`] for directions on how to switch between the two storage implementations.
This allows you to choose the storage mode that best fits your needs for development or production.

### Previous: In-Memory Storage
Initially, the application used an in-memory repository (`InMemoryReportRepository`) to store reports and attachments. All data was kept in server memory and lost when the application restarted. Attachments were stored as base64-encoded strings, and all report data was managed using thread-safe collections.

- **Reports and Attachments:** Stored in `ConcurrentDictionary` objects.
- **Attachments:** Uploaded files were converted to base64 strings and associated with their reports.
- **No Persistence:** Data was not saved to disk or a database; it was only available during the application's runtime.

### Current: Database Storage with Entity Framework Core
The application now uses a database-backed repository for data persistence. All reports, attachments, categories, and users are stored in a SQL database via Entity Framework Core (`AppDbContext`). This ensures that data survives application restarts and supports scalability.

- **Persistence:** Data is saved in a relational database and remains available after restarts.
- **EF Core:** The repository implementation uses `AppDbContext` for all CRUD operations.
- **Attachments:** Uploaded files are stored in the database and associated with their reports.
- **Production-Ready:** The application is now suitable for real-world use with robust data storage.

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

**Note:** You can switch between in-memory and database storage by editing the service registrations in [`Program.cs`]. All data is persistent when using the database option.