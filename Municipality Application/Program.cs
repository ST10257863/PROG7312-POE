using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Municipality_Application.Data;
using Municipality_Application.Data.EF;
using Municipality_Application.Data.InMemory;
using Municipality_Application.Interfaces;
using Municipality_Application.Interfaces.Service;
using Municipality_Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Toggle this flag to switch between in-memory and EF Core modes
bool useInMemory = true; // Set to true for in-memory mode

if (useInMemory)
{
    // In-memory repositories (data lost on restart)
    builder.Services.AddSingleton<IEventRepository, InMemoryEventRepository>();
    builder.Services.AddSingleton<IReportRepository, InMemoryReportRepository>();
    builder.Services.AddSingleton<ICategoryRepository, InMemoryCategoryRepository>();
    builder.Services.AddSingleton<InMemoryDataSeeder>();
}
else
{
    // EF Core with LocalDB (persistent)
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("ApplicationDbConnection")));
    builder.Services.AddScoped<IEventRepository, EfEventRepository>();
    builder.Services.AddScoped<IReportRepository, EfReportRepository>();
    builder.Services.AddScoped<ICategoryRepository, EfCategoryRepository>();
    builder.Services.AddScoped<EFDataSeeder>();
}

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
    if (useInMemory)
    {
        var seeder = scope.ServiceProvider.GetRequiredService<InMemoryDataSeeder>();
        await seeder.SeedAllAsync();
    }
    else
    {
        // Ensure database is created and migrations are applied
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();

        // Seed the database
        var seeder = scope.ServiceProvider.GetRequiredService<EFDataSeeder>();
        await seeder.SeedAllAsync();
    }
}

var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

app.Run();