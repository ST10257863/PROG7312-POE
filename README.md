# PROG7312POE

## Overview
This application is a municipal issue reporting system built with ASP.NET Core Razor Pages. It enables residents to report local issues (such as roads, water, electricity, sanitation, etc.), attach files (images or documents), and receive confirmation and status updates. The user interface is designed to be clear, consistent, and user-friendly, with feedback and engagement features to encourage participation.

## Features
- **Main Menu:** On startup, users see three options: Report Issues (enabled), Local Events and Announcements (disabled), and Service Request Status (disabled).
- **Report Issues:** Users can provide location, select a category, describe the issue, and attach images or documents. The interface includes feedback and engagement elements.
- **Attachments:** Users can upload images or documents related to their report.

## In-Memory Storage (Current Implementation)
Currently, the application uses an in-memory repository (`InMemoryReportRepository`) to store reports and attachments. All data is kept in server memory and will be lost when the application restarts. Attachments are stored as base64-encoded strings, and all report data is managed using thread-safe collections.

- **Reports and Attachments:** Stored in `ConcurrentDictionary` objects.
- **Attachments:** Uploaded files are converted to base64 strings and associated with their reports.
- **No Persistence:** Data is not saved to disk or a database; it is only available during the application's runtime.

## Future: Database Integration (Part 2)
In the next phase, the in-memory storage will be replaced by database calls using Entity Framework Core. The `AppDbContext` class and related models are already defined to support this transition. Reports, attachments, categories, and users will be stored in a SQL database, providing data persistence and scalability.

- **Persistence:** Data will be saved in a relational database and survive application restarts.
- **EF Core:** The repository will be updated to use `AppDbContext` for all CRUD operations.
- **Seamless Transition:** The application's logic and UI will remain similar, but storage will be more robust and production-ready.

## How to Run
1. Clone the repository.
2. Open the solution in Visual Studio.
3. Build and run the project.
4. Use the web interface to report issues and attach files.

---

**Note:** This project is currently in the in-memory storage phase. All data will be lost when the app stops. Database integration is planned for the next part.