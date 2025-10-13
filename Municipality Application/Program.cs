using Microsoft.EntityFrameworkCore;
using Municipality_Application.Data;
using Municipality_Application.Interfaces;
using Municipality_Application.Services; // Add this for EventService

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add EF Core with LocalDB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ApplicationDbConnection")));

// For in-memory
builder.Services.AddSingleton<IEventRepository, InMemoryEventRepository>();
builder.Services.AddSingleton<IReportRepository, InMemoryReportRepository>();

// For EF Core
// builder.Services.AddScoped<IEventRepository, EfEventRepository>();
// builder.Services.AddScoped<IReportRepository, EfReportRepository>();

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

app.Run();