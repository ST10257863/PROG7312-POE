using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Municipality_Application.Data;
using Municipality_Application.Data.InMemory;
using Municipality_Application.Interfaces;
using Municipality_Application.Interfaces.Service;
using Municipality_Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add EF Core with LocalDB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ApplicationDbConnection")));

/*
    Data Storage Configuration:
    You can switch between in-memory and database-backed (EF Core) repositories by commenting or uncommenting the lines below.

    - For in-memory storage (data is lost on application restart), uncomment the AddSingleton registrations and comment out the AddScoped EF Core registrations.
    - For persistent database storage (data is saved in SQL via Entity Framework Core), ensure the AddScoped EF Core registrations are uncommented and the in-memory lines are commented.

    This allows you to easily change the application's data storage mode for development or production needs.
*/
//For in-memory
builder.Services.AddSingleton<IEventRepository, InMemoryEventRepository>();
builder.Services.AddSingleton<IReportRepository, InMemoryReportRepository>();
builder.Services.AddSingleton<ICategoryRepository, InMemoryCategoryRepository>();

// For EF Core
//builder.Services.AddScoped<IEventRepository, EfEventRepository>();
//builder.Services.AddScoped<IReportRepository, EfReportRepository>();
//builder.Services.AddScoped<ICategoryRepository, EfCategoryRepository>();

builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IReportService, ReportService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var categoryRepo = scope.ServiceProvider.GetRequiredService<ICategoryRepository>();
    await categoryRepo.SeedDefaultCategoriesAsync();

    var eventRepo = scope.ServiceProvider.GetRequiredService<IEventRepository>();
    await eventRepo.SeedDefaultEventsAsync();
}

var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

app.Run();