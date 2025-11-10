using Microsoft.EntityFrameworkCore;
using Municipality_Application.Data;
using Municipality_Application.Interfaces.Seeding;
using Municipality_Application.Models;

namespace Municipality_Application.Services
{
    public class EFDataSeeder : IDataSeeder
    {
        private readonly AppDbContext _dbContext;
        private readonly Random _random = new();

        public EFDataSeeder(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SeedAllAsync()
        {
            await SeedDepartmentsAsync();
            await SeedCategoriesAsync();
            await SeedAddressesAsync();
            await SeedEventsAsync();
            await SeedReportsAsync();
        }

        private async Task SeedDepartmentsAsync()
        {
            if (!await _dbContext.Departments.AnyAsync())
            {
                var departments = new List<Department>
                {
                    new Department { Name = "Roads & Transport" },
                    new Department { Name = "Water & Sanitation" },
                    new Department { Name = "Electricity" },
                    new Department { Name = "Parks & Recreation" },
                    new Department { Name = "Waste Management" },
                    new Department { Name = "Public Safety" },
                    new Department { Name = "Housing" }
                };
                _dbContext.Departments.AddRange(departments);
                await _dbContext.SaveChangesAsync();
            }
        }

        private async Task SeedCategoriesAsync()
        {
            if (!await _dbContext.Categories.AnyAsync())
            {
                var departments = await _dbContext.Departments.ToListAsync();
                var categories = new List<Category>
                {
                    new Category { Name = "Pothole", DepartmentId = departments.First(d => d.Name == "Roads & Transport").Id },
                    new Category { Name = "Streetlight Out", DepartmentId = departments.First(d => d.Name == "Electricity").Id },
                    new Category { Name = "Water Leak", DepartmentId = departments.First(d => d.Name == "Water & Sanitation").Id },
                    new Category { Name = "Blocked Drain", DepartmentId = departments.First(d => d.Name == "Water & Sanitation").Id },
                    new Category { Name = "Illegal Dumping", DepartmentId = departments.First(d => d.Name == "Waste Management").Id },
                    new Category { Name = "Park Maintenance", DepartmentId = departments.First(d => d.Name == "Parks & Recreation").Id },
                    new Category { Name = "Vandalism", DepartmentId = departments.First(d => d.Name == "Public Safety").Id },
                    new Category { Name = "Housing Complaint", DepartmentId = departments.First(d => d.Name == "Housing").Id }
                };
                _dbContext.Categories.AddRange(categories);
                await _dbContext.SaveChangesAsync();
            }
        }

        private async Task SeedAddressesAsync()
        {
            if (!await _dbContext.Addresses.AnyAsync())
            {
                // Real Western Cape suburbs
                var suburbs = new[]
                {
                    "Sea Point", "Claremont", "Rondebosch", "Durbanville", "Bellville", "Constantia", "Khayelitsha", "Mitchells Plain",
                    "Muizenberg", "Hout Bay", "Parow", "Goodwood", "Somerset West", "Stellenbosch", "Paarl", "Brackenfell", "Wynberg",
                    "Observatory", "Green Point", "Woodstock", "Table View", "Milnerton", "Kraaifontein", "Fish Hoek", "Simons Town",
                    "Noordhoek", "Tokai", "Bishopscourt", "Elsies River", "Mowbray", "Athlone", "Pinelands", "Retreat", "Epping"
                };

                var streets = new[]
                {
                    "Main Road", "Victoria Road", "High Street", "Long Street", "Adderley Street", "Loop Street", "Buitenkant Street",
                    "Voortrekker Road", "Somerset Road", "Kloof Street", "Durban Road", "Cape Road", "Church Street", "Beach Road", "Park Avenue"
                };

                var addresses = new List<Address>();
                var city = "Cape Town";
                var province = "Western Cape";
                var country = "South Africa";

                for (int i = 0; i < 200; i++)
                {
                    var suburb = suburbs[_random.Next(suburbs.Length)];
                    var street = $"{_random.Next(1, 500)} {streets[_random.Next(streets.Length)]}";
                    // approximate lat/lng variation around Western Cape region
                    var lat = -33.7 + _random.NextDouble() * 0.6;
                    var lng = 18.3 + _random.NextDouble() * 1.2;
                    addresses.Add(new Address
                    {
                        Street = street,
                        Suburb = suburb,
                        City = city,
                        Province = province,
                        Country = country,
                        Latitude = Math.Round(lat, 6),
                        Longitude = Math.Round(lng, 6),
                        FormattedAddress = $"{street}, {suburb}, {city}"
                    });
                }

                _dbContext.Addresses.AddRange(addresses);
                await _dbContext.SaveChangesAsync();
            }
        }

        private async Task SeedEventsAsync()
        {
            if (!await _dbContext.Events.AnyAsync())
            {
                var addresses = await _dbContext.Addresses.ToListAsync();
                var eventCategories = new[]
                {
                    "Maintenance", "Meeting", "Festival", "Sports", "Market", "Fair", "Music", "Charity",
                    "Tech", "Health", "Food & Drink", "Film", "Culture", "Education", "Nature"
                };

                var events = new List<Event>();
                for (int i = 0; i < 1000; i++)
                {
                    var addr = addresses[_random.Next(addresses.Count)];
                    var category = eventCategories[_random.Next(eventCategories.Length)];
                    var priority = _random.Next(1, 4);
                    var daysAhead = _random.Next(-30, 90);
                    var date = DateTime.Today.AddDays(daysAhead);

                    events.Add(new Event
                    {
                        Title = $"{category} Event {i}",
                        Description = $"Auto-generated {category.ToLower()} event in {addr.Suburb}.",
                        Category = category,
                        Date = date,
                        Address = addr,
                        Priority = priority
                    });
                }

                // Bulk insert to improve performance
                _dbContext.Events.AddRange(events);
                await _dbContext.SaveChangesAsync();
            }
        }

        private async Task SeedReportsAsync()
        {
            if (!await _dbContext.Reports.AnyAsync())
            {
                var addresses = await _dbContext.Addresses.ToListAsync();
                var categories = await _dbContext.Categories.ToListAsync();
                var reportCount = _random.Next(2000, 5001);

                var reports = new List<Report>(reportCount);

                for (int i = 0; i < reportCount; i++)
                {
                    var address = addresses[_random.Next(addresses.Count)];
                    var category = categories[_random.Next(categories.Count)];
                    var reportedDate = DateTime.UtcNow.AddDays(_random.Next(-60, 30));
                    var status = (IssueStatus)_random.Next(Enum.GetValues(typeof(IssueStatus)).Length);

                    reports.Add(new Report
                    {
                        Id = Guid.NewGuid(),
                        ReportedAt = reportedDate,
                        CategoryId = category.Id,
                        Description = $"Auto-generated report about {category.Name.ToLower()} at {address.Suburb}.",
                        AddressId = address.Id,
                        Status = status
                    });
                }

                _dbContext.Reports.AddRange(reports);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
