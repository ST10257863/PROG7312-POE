using Municipality_Application.Interfaces;
using Municipality_Application.Interfaces.Seeding;
using Municipality_Application.Models;

namespace Municipality_Application.Services
{
    public class InMemoryDataSeeder : IDataSeeder
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IReportRepository _reportRepository;
        private readonly Random _random = new();

        public InMemoryDataSeeder(
            ICategoryRepository categoryRepository,
            IEventRepository eventRepository,
            IReportRepository reportRepository)
        {
            _categoryRepository = categoryRepository;
            _eventRepository = eventRepository;
            _reportRepository = reportRepository;
        }

        public async Task SeedAllAsync()
        {
            var categories = GenerateCategories();
            await (_categoryRepository as dynamic).SetAllCategoriesAsync(categories);

            var addresses = GenerateAddresses();
            var events = GenerateEvents(categories, addresses);
            await (_eventRepository as dynamic).SetAllEventsAsync(events);

            var reports = GenerateReports(categories, addresses);
            await (_reportRepository as dynamic).SetAllCategoriesAsync(categories);
            await (_reportRepository as dynamic).SetAllReportsAsync(reports);
        }

        private List<Category> GenerateCategories()
        {
            return new List<Category>
            {
                new Category { Name = "Pothole" },
                new Category { Name = "Streetlight Out" },
                new Category { Name = "Water Leak" },
                new Category { Name = "Blocked Drain" },
                new Category { Name = "Illegal Dumping" },
                new Category { Name = "Park Maintenance" },
                new Category { Name = "Vandalism" },
                new Category { Name = "Housing Complaint" }
            };
        }

        private List<Address> GenerateAddresses()
        {
            var addresses = new List<Address>();
            var streets = new[] { "Main St", "High St", "Church St", "Station Rd", "Market St", "Park Ave", "Victoria Rd", "Long St", "Short St", "Oak Ave" };
            var suburbs = new[] { "Central", "West End", "East Side", "North Park", "South Hills" };
            var city = "Cape Town";
            var province = "Western Cape";
            var country = "South Africa";

            for (int i = 1; i <= 30; i++)
            {
                var street = $"{_random.Next(1, 200)} {streets[_random.Next(streets.Length)]}";
                var suburb = suburbs[_random.Next(suburbs.Length)];
                var lat = -33.9 + _random.NextDouble() * 0.1;
                var lng = 18.4 + _random.NextDouble() * 0.1;
                addresses.Add(new Address
                {
                    Id = i,
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
            return addresses;
        }

        private List<Event> GenerateEvents(List<Category> categories, List<Address> addresses)
        {
            // Ensure there are enough addresses
            if (addresses.Count < 29)
                return new List<Event>();

            return new List<Event>
            {
                new Event { Title = "Water Maintenance", Description = "Scheduled water pipe maintenance.", Category = "Maintenance", Date = DateTime.Today.AddDays(1), AddressId = addresses[0].Id, Address = addresses[0], Priority = 2 },
                new Event { Title = "Road Maintenance", Description = "Scheduled road repair.", Category = "Maintenance", Date = DateTime.Today.AddDays(21), AddressId = addresses[1].Id, Address = addresses[1], Priority = 1 },
                new Event { Title = "Community Meeting", Description = "Monthly community meeting.", Category = "Meeting", Date = DateTime.Today.AddDays(3), AddressId = addresses[2].Id, Address = addresses[2], Priority = 1 },
                new Event { Title = "Neighborhood Meeting", Description = "Neighborhood watch meeting.", Category = "Meeting", Date = DateTime.Today.AddDays(22), AddressId = addresses[3].Id, Address = addresses[3], Priority = 2 },
                new Event { Title = "Festival", Description = "Annual city festival.", Category = "Festival", Date = DateTime.Today.AddDays(7), AddressId = addresses[4].Id, Address = addresses[4], Priority = 3 },
                new Event { Title = "Spring Festival", Description = "Spring celebration festival.", Category = "Festival", Date = DateTime.Today.AddDays(23), AddressId = addresses[5].Id, Address = addresses[5], Priority = 2 },
                new Event { Title = "Sports Day", Description = "Local sports event.", Category = "Sports", Date = DateTime.Today.AddDays(2), AddressId = addresses[6].Id, Address = addresses[6], Priority = 2 },
                new Event { Title = "Football Match", Description = "Local football match.", Category = "Sports", Date = DateTime.Today.AddDays(24), AddressId = addresses[6].Id, Address = addresses[6], Priority = 1 },
                new Event { Title = "Art Exhibition", Description = "Contemporary art display.", Category = "Art", Date = DateTime.Today.AddDays(5), AddressId = addresses[7].Id, Address = addresses[7], Priority = 1 },
                new Event { Title = "Craft Workshop", Description = "Handmade crafts workshop.", Category = "Art", Date = DateTime.Today.AddDays(19), AddressId = addresses[8].Id, Address = addresses[8], Priority = 1 },
                new Event { Title = "Food Market", Description = "Weekly food market.", Category = "Market", Date = DateTime.Today.AddDays(4), AddressId = addresses[9].Id, Address = addresses[9], Priority = 2 },
                new Event { Title = "Farmers Market", Description = "Fresh produce market.", Category = "Market", Date = DateTime.Today.AddDays(11), AddressId = addresses[10].Id, Address = addresses[10], Priority = 2 },
                new Event { Title = "Book Fair", Description = "Annual book fair.", Category = "Fair", Date = DateTime.Today.AddDays(6), AddressId = addresses[11].Id, Address = addresses[11], Priority = 1 },
                new Event { Title = "Science Fair", Description = "School science fair.", Category = "Fair", Date = DateTime.Today.AddDays(25), AddressId = addresses[12].Id, Address = addresses[12], Priority = 2 },
                new Event { Title = "Music Concert", Description = "Live music event.", Category = "Music", Date = DateTime.Today.AddDays(8), AddressId = addresses[13].Id, Address = addresses[13], Priority = 3 },
                new Event { Title = "Jazz Night", Description = "Live jazz concert.", Category = "Music", Date = DateTime.Today.AddDays(26), AddressId = addresses[14].Id, Address = addresses[14], Priority = 2 },
                new Event { Title = "Charity Run", Description = "5km charity run.", Category = "Charity", Date = DateTime.Today.AddDays(9), AddressId = addresses[15].Id, Address = addresses[15], Priority = 2 },
                new Event { Title = "Beach Cleanup", Description = "Community beach cleanup.", Category = "Charity", Date = DateTime.Today.AddDays(14), AddressId = addresses[16].Id, Address = addresses[16], Priority = 2 },
                new Event { Title = "Tech Meetup", Description = "Monthly tech meetup.", Category = "Tech", Date = DateTime.Today.AddDays(10), AddressId = addresses[17].Id, Address = addresses[17], Priority = 1 },
                new Event { Title = "Startup Pitch", Description = "Startup pitch event.", Category = "Tech", Date = DateTime.Today.AddDays(27), AddressId = addresses[18].Id, Address = addresses[18], Priority = 2 },
                new Event { Title = "Yoga in the Park", Description = "Outdoor yoga session.", Category = "Health", Date = DateTime.Today.AddDays(12), AddressId = addresses[19].Id, Address = addresses[19], Priority = 1 },
                new Event { Title = "Health Expo", Description = "Community health expo.", Category = "Health", Date = DateTime.Today.AddDays(28), AddressId = addresses[3].Id, Address = addresses[3], Priority = 2 },
                new Event { Title = "Wine Tasting", Description = "Local wine tasting event.", Category = "Food & Drink", Date = DateTime.Today.AddDays(13), AddressId = addresses[20].Id, Address = addresses[20], Priority = 3 },
                new Event { Title = "Beer Festival", Description = "Craft beer festival.", Category = "Food & Drink", Date = DateTime.Today.AddDays(29), AddressId = addresses[21].Id, Address = addresses[21], Priority = 2 },
                new Event { Title = "Cycling Race", Description = "Annual cycling race.", Category = "Sports", Date = DateTime.Today.AddDays(15), AddressId = addresses[22].Id, Address = addresses[22], Priority = 1 },
                new Event { Title = "Marathon", Description = "City marathon event.", Category = "Sports", Date = DateTime.Today.AddDays(30), AddressId = addresses[1].Id, Address = addresses[1], Priority = 2 },
                new Event { Title = "Film Screening", Description = "Outdoor film screening.", Category = "Film", Date = DateTime.Today.AddDays(16), AddressId = addresses[23].Id, Address = addresses[23], Priority = 2 },
                new Event { Title = "Documentary Night", Description = "Documentary film screening.", Category = "Film", Date = DateTime.Today.AddDays(31), AddressId = addresses[23].Id, Address = addresses[23], Priority = 1 },
                new Event { Title = "Heritage Walk", Description = "Guided heritage walk.", Category = "Culture", Date = DateTime.Today.AddDays(17), AddressId = addresses[24].Id, Address = addresses[24], Priority = 1 },
                new Event { Title = "Cultural Parade", Description = "Annual cultural parade.", Category = "Culture", Date = DateTime.Today.AddDays(32), AddressId = addresses[1].Id, Address = addresses[1], Priority = 2 },
                new Event { Title = "Science Expo", Description = "School science expo.", Category = "Education", Date = DateTime.Today.AddDays(18), AddressId = addresses[12].Id, Address = addresses[12], Priority = 2 },
                new Event { Title = "Education Seminar", Description = "Education improvement seminar.", Category = "Education", Date = DateTime.Today.AddDays(33), AddressId = addresses[25].Id, Address = addresses[25], Priority = 1 },
                new Event { Title = "Nature Hike", Description = "Guided nature hike.", Category = "Nature", Date = DateTime.Today.AddDays(20), AddressId = addresses[26].Id, Address = addresses[26], Priority = 3 },
                new Event { Title = "Bird Watching", Description = "Bird watching event.", Category = "Nature", Date = DateTime.Today.AddDays(34), AddressId = addresses[26].Id, Address = addresses[26], Priority = 2 }
            };
        }

        private List<Report> GenerateReports(List<Category> categories, List<Address> addresses)
        {
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
                    Category = category,
                    Description = $"Auto-generated report for {category.Name.ToLower()}",
                    AddressId = address.Id,
                    Address = address,
                    Status = IssueStatus.Reported
                });
            }
            return reports;
        }
    }
}