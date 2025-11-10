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
                var streets = new[] { "Main St", "High St", "Church St", "Station Rd", "Market St", "Park Ave", "Victoria Rd", "Long St", "Short St", "Oak Ave" };
                var suburbs = new[] { "Central", "West End", "East Side", "North Park", "South Hills" };
                var city = "Cape Town";
                var province = "Western Cape";
                var country = "South Africa";

                var addresses = new List<Address>();
                for (int i = 1; i <= 30; i++)
                {
                    var street = $"{_random.Next(1, 200)} {streets[_random.Next(streets.Length)]}";
                    var suburb = suburbs[_random.Next(suburbs.Length)];
                    var lat = -33.9 + _random.NextDouble() * 0.1;
                    var lng = 18.4 + _random.NextDouble() * 0.1;
                    addresses.Add(new Address
                    {
                        Street = street,
                        Suburb = suburb,
                        City = city,
                        Province = province,
                        Country = country,
                        Latitude = lat,
                        Longitude = lng,
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

                // Ensure there are enough addresses
                if (addresses.Count < 29)
                    return;

                var sampleEvents = new List<Event>
                {
                    new Event { Title = "Water Maintenance", Description = "Scheduled water pipe maintenance.", Category = "Maintenance", Date = DateTime.Today.AddDays(1), Address = addresses[0], Priority = 2 },
                    new Event { Title = "Road Maintenance", Description = "Scheduled road repair.", Category = "Maintenance", Date = DateTime.Today.AddDays(21), Address = addresses[1], Priority = 1 },
                    new Event { Title = "Community Meeting", Description = "Monthly community meeting.", Category = "Meeting", Date = DateTime.Today.AddDays(3), Address = addresses[2], Priority = 1 },
                    new Event { Title = "Neighborhood Meeting", Description = "Neighborhood watch meeting.", Category = "Meeting", Date = DateTime.Today.AddDays(22), Address = addresses[3], Priority = 2 },
                    new Event { Title = "Festival", Description = "Annual city festival.", Category = "Festival", Date = DateTime.Today.AddDays(7), Address = addresses[4], Priority = 3 },
                    new Event { Title = "Spring Festival", Description = "Spring celebration festival.", Category = "Festival", Date = DateTime.Today.AddDays(23), Address = addresses[5], Priority = 2 },
                    new Event { Title = "Sports Day", Description = "Local sports event.", Category = "Sports", Date = DateTime.Today.AddDays(2), Address = addresses[6], Priority = 2 },
                    new Event { Title = "Football Match", Description = "Local football match.", Category = "Sports", Date = DateTime.Today.AddDays(24), Address = addresses[6], Priority = 1 },
                    new Event { Title = "Art Exhibition", Description = "Contemporary art display.", Category = "Art", Date = DateTime.Today.AddDays(5), Address = addresses[7], Priority = 1 },
                    new Event { Title = "Craft Workshop", Description = "Handmade crafts workshop.", Category = "Art", Date = DateTime.Today.AddDays(19), Address = addresses[8], Priority = 1 },
                    new Event { Title = "Food Market", Description = "Weekly food market.", Category = "Market", Date = DateTime.Today.AddDays(4), Address = addresses[9], Priority = 2 },
                    new Event { Title = "Farmers Market", Description = "Fresh produce market.", Category = "Market", Date = DateTime.Today.AddDays(11), Address = addresses[10], Priority = 2 },
                    new Event { Title = "Book Fair", Description = "Annual book fair.", Category = "Fair", Date = DateTime.Today.AddDays(6), Address = addresses[11], Priority = 1 },
                    new Event { Title = "Science Fair", Description = "School science fair.", Category = "Fair", Date = DateTime.Today.AddDays(25), Address = addresses[12], Priority = 2 },
                    new Event { Title = "Music Concert", Description = "Live music event.", Category = "Music", Date = DateTime.Today.AddDays(8), Address = addresses[13], Priority = 3 },
                    new Event { Title = "Jazz Night", Description = "Live jazz concert.", Category = "Music", Date = DateTime.Today.AddDays(26), Address = addresses[14], Priority = 2 },
                    new Event { Title = "Charity Run", Description = "5km charity run.", Category = "Charity", Date = DateTime.Today.AddDays(9), Address = addresses[15], Priority = 2 },
                    new Event { Title = "Beach Cleanup", Description = "Community beach cleanup.", Category = "Charity", Date = DateTime.Today.AddDays(14), Address = addresses[16], Priority = 2 },
                    new Event { Title = "Tech Meetup", Description = "Monthly tech meetup.", Category = "Tech", Date = DateTime.Today.AddDays(10), Address = addresses[17], Priority = 1 },
                    new Event { Title = "Startup Pitch", Description = "Startup pitch event.", Category = "Tech", Date = DateTime.Today.AddDays(27), Address = addresses[18], Priority = 2 },
                    new Event { Title = "Yoga in the Park", Description = "Outdoor yoga session.", Category = "Health", Date = DateTime.Today.AddDays(12), Address = addresses[19], Priority = 1 },
                    new Event { Title = "Health Expo", Description = "Community health expo.", Category = "Health", Date = DateTime.Today.AddDays(28), Address = addresses[3], Priority = 2 },
                    new Event { Title = "Wine Tasting", Description = "Local wine tasting event.", Category = "Food & Drink", Date = DateTime.Today.AddDays(13), Address = addresses[20], Priority = 3 },
                    new Event { Title = "Beer Festival", Description = "Craft beer festival.", Category = "Food & Drink", Date = DateTime.Today.AddDays(29), Address = addresses[21], Priority = 2 },
                    new Event { Title = "Cycling Race", Description = "Annual cycling race.", Category = "Sports", Date = DateTime.Today.AddDays(15), Address = addresses[22], Priority = 1 },
                    new Event { Title = "Marathon", Description = "City marathon event.", Category = "Sports", Date = DateTime.Today.AddDays(30), Address = addresses[1], Priority = 2 },
                    new Event { Title = "Film Screening", Description = "Outdoor film screening.", Category = "Film", Date = DateTime.Today.AddDays(16), Address = addresses[23], Priority = 2 },
                    new Event { Title = "Documentary Night", Description = "Documentary film screening.", Category = "Film", Date = DateTime.Today.AddDays(31), Address = addresses[23], Priority = 1 },
                    new Event { Title = "Heritage Walk", Description = "Guided heritage walk.", Category = "Culture", Date = DateTime.Today.AddDays(17), Address = addresses[24], Priority = 1 },
                    new Event { Title = "Cultural Parade", Description = "Annual cultural parade.", Category = "Culture", Date = DateTime.Today.AddDays(32), Address = addresses[1], Priority = 2 },
                    new Event { Title = "Science Expo", Description = "School science expo.", Category = "Education", Date = DateTime.Today.AddDays(18), Address = addresses[12], Priority = 2 },
                    new Event { Title = "Education Seminar", Description = "Education improvement seminar.", Category = "Education", Date = DateTime.Today.AddDays(33), Address = addresses[25], Priority = 1 },
                    new Event { Title = "Nature Hike", Description = "Guided nature hike.", Category = "Nature", Date = DateTime.Today.AddDays(20), Address = addresses[26], Priority = 3 },
                    new Event { Title = "Bird Watching", Description = "Bird watching event.", Category = "Nature", Date = DateTime.Today.AddDays(34), Address = addresses[26], Priority = 2 }
                };

                _dbContext.Events.AddRange(sampleEvents);
                await _dbContext.SaveChangesAsync();
            }
        }

        private async Task SeedReportsAsync()
        {
            if (!await _dbContext.Reports.AnyAsync())
            {
                var addresses = await _dbContext.Addresses.ToListAsync();
                var categories = await _dbContext.Categories.ToListAsync();
                var reports = new List<Report>();
                for (int i = 0; i < 30; i++)
                {
                    var address = addresses[_random.Next(addresses.Count)];
                    var category = categories[_random.Next(categories.Count)];
                    var date = DateTime.UtcNow.AddDays(_random.Next(-10, 30));
                    reports.Add(new Report
                    {
                        Id = Guid.NewGuid(),
                        ReportedAt = date,
                        CategoryId = category.Id,
                        Description = $"Auto-generated report for {category.Name.ToLower()}",
                        AddressId = address.Id,
                        Status = IssueStatus.Reported
                    });
                }
                _dbContext.Reports.AddRange(reports);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}