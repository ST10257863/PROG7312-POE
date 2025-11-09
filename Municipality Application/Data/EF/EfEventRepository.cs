using Municipality_Application.Interfaces;
using Municipality_Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Municipality_Application.Data.EF
{
    /// <summary>
    /// Entity Framework Core implementation of <see cref="IEventRepository"/> for managing event data in the database.
    /// </summary>
    public class EfEventRepository : IEventRepository
    {
        private readonly AppDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="EfEventRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The application's database context.</param>
        public EfEventRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Adds a new event to the database, including its address.
        /// </summary>
        /// <param name="ev">The event to add.</param>
        /// <returns>The added <see cref="Event"/> entity.</returns>
        public async Task<Event> AddEventAsync(Event ev)
        {
            // Ensure Address is added and tracked
            if (ev.Address != null)
            {
                if (ev.Address.Id == 0)
                {
                    _dbContext.Addresses.Add(ev.Address);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    if (!_dbContext.Addresses.Local.Any(a => a.Id == ev.Address.Id))
                        _dbContext.Addresses.Attach(ev.Address);
                }
                ev.AddressId = ev.Address.Id;
            }

            _dbContext.Events.Add(ev);
            await _dbContext.SaveChangesAsync();
            return ev;
        }

        /// <summary>
        /// Retrieves an event by its unique identifier, including its address.
        /// </summary>
        /// <param name="id">The event's unique identifier.</param>
        /// <returns>The <see cref="Event"/> entity if found; otherwise, null.</returns>
        public async Task<Event?> GetEventByIdAsync(int id)
        {
            return await _dbContext.Events
                .Include(e => e.Address)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        /// <summary>
        /// Retrieves all events from the database, including their addresses.
        /// </summary>
        /// <returns>A list of all <see cref="Event"/> entities.</returns>
        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            return await _dbContext.Events
                .Include(e => e.Address)
                .ToListAsync();
        }

        /// <summary>
        /// Updates an existing event in the database, including its address.
        /// </summary>
        /// <param name="ev">The event with updated data.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public async Task<bool> UpdateEventAsync(Event ev)
        {
            var existing = await _dbContext.Events
                .Include(e => e.Address)
                .FirstOrDefaultAsync(e => e.Id == ev.Id);
            if (existing == null)
                return false;

            // Update Address
            if (ev.Address != null)
            {
                if (existing.Address == null || existing.Address.Id != ev.Address.Id)
                {
                    _dbContext.Addresses.Add(ev.Address);
                    await _dbContext.SaveChangesAsync();
                    ev.AddressId = ev.Address.Id;
                }
                else
                {
                    _dbContext.Entry(existing.Address).CurrentValues.SetValues(ev.Address);
                }
            }

            _dbContext.Entry(existing).CurrentValues.SetValues(ev);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Deletes an event from the database, optionally removing its address if not referenced elsewhere.
        /// </summary>
        /// <param name="id">The event's unique identifier.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        public async Task<bool> DeleteEventAsync(int id)
        {
            var ev = await _dbContext.Events
                .Include(e => e.Address)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (ev == null)
                return false;

            _dbContext.Events.Remove(ev);

            // Optionally remove the address if not referenced by any other event
            if (ev.AddressId != 0)
            {
                bool addressInUse = await _dbContext.Events.AnyAsync(e => e.AddressId == ev.AddressId && e.Id != id);
                if (!addressInUse && ev.Address != null)
                {
                    _dbContext.Addresses.Remove(ev.Address);
                }
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Increments the search frequency for a given search term.
        /// </summary>
        /// <param name="searchTerm">The search term to increment.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task IncrementSearchFrequencyAsync(string searchTerm)
        {
            var key = searchTerm.ToLower();
            var entry = await _dbContext.EventSearchFrequencies.FindAsync(key);
            if (entry == null)
            {
                entry = new EventSearchFrequency { SearchTerm = key, Frequency = 1 };
                _dbContext.EventSearchFrequencies.Add(entry);
            }
            else
            {
                entry.Frequency += 1;
                _dbContext.EventSearchFrequencies.Update(entry);
            }
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves the search frequency dictionary for all search terms.
        /// </summary>
        /// <returns>A dictionary mapping search terms to their frequency.</returns>
        public async Task<Dictionary<string, int>> GetSearchFrequencyAsync()
        {
            return await _dbContext.EventSearchFrequencies
                .ToDictionaryAsync(e => e.SearchTerm, e => e.Frequency);
        }

        /// <summary>
        /// Seeds the database with default events if none exist, including addresses.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SeedDefaultEventsAsync()
        {
            if (!await _dbContext.Events.AnyAsync())
            {
                // --- ADDRESS SEEDING ---
                var addresses = new List<Address>
                {
                    new Address { Street = "123 Main Street", Suburb = "Cape Town CBD", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9249, Longitude = 18.4241, FormattedAddress = "123 Main Street, Cape Town CBD" },
                    new Address { Street = "456 Main Road", Suburb = "Cape Town CBD", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9250, Longitude = 18.4250, FormattedAddress = "456 Main Road, Cape Town CBD" },
                    new Address { Street = "Town Hall", Suburb = "Cape Town CBD", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9258, Longitude = 18.4232, FormattedAddress = "Town Hall, Cape Town CBD" },
                    new Address { Street = "Community Center", Suburb = "Cape Town CBD", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9260, Longitude = 18.4235, FormattedAddress = "Community Center, Cape Town CBD" },
                    new Address { Street = "Central Park", Suburb = "Cape Town CBD", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9270, Longitude = 18.4220, FormattedAddress = "Central Park, Cape Town CBD" },
                    new Address { Street = "Botanical Gardens", Suburb = "Cape Town CBD", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9275, Longitude = 18.4225, FormattedAddress = "Botanical Gardens, Cape Town CBD" },
                    new Address { Street = "Green Point Stadium", Suburb = "Green Point", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9066, Longitude = 18.4114, FormattedAddress = "Green Point Stadium, Green Point" },
                    new Address { Street = "Zeitz MOCAA", Suburb = "V&A Waterfront", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9079, Longitude = 18.4206, FormattedAddress = "Zeitz MOCAA, V&A Waterfront" },
                    new Address { Street = "Fish Hoek Community Centre", Suburb = "Fish Hoek", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -34.1277, Longitude = 18.4326, FormattedAddress = "Fish Hoek Community Centre, Fish Hoek" },
                    new Address { Street = "Old Biscuit Mill", Suburb = "Woodstock", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9281, Longitude = 18.4489, FormattedAddress = "Old Biscuit Mill, Woodstock" },
                    new Address { Street = "Oranjezicht City Farm", Suburb = "Oranjezicht", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9366, Longitude = 18.4137, FormattedAddress = "Oranjezicht City Farm, Oranjezicht" },
                    new Address { Street = "Claremont Library", Suburb = "Claremont", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9781, Longitude = 18.4655, FormattedAddress = "Claremont Library, Claremont" },
                    new Address { Street = "Pinelands High School", Suburb = "Pinelands", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9362, Longitude = 18.5057, FormattedAddress = "Pinelands High School, Pinelands" },
                    new Address { Street = "Kirstenbosch Gardens", Suburb = "Newlands", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9881, Longitude = 18.4321, FormattedAddress = "Kirstenbosch Gardens, Newlands" },
                    new Address { Street = "Jazz Club", Suburb = "Cape Town CBD", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9240, Longitude = 18.4245, FormattedAddress = "Jazz Club, Cape Town CBD" },
                    new Address { Street = "Sea Point Promenade", Suburb = "Sea Point", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9121, Longitude = 18.3872, FormattedAddress = "Sea Point Promenade, Sea Point" },
                    new Address { Street = "Muizenberg Beach", Suburb = "Muizenberg", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -34.1052, Longitude = 18.4693, FormattedAddress = "Muizenberg Beach, Muizenberg" },
                    new Address { Street = "Century City Conference Centre", Suburb = "Century City", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.8925, Longitude = 18.5113, FormattedAddress = "Century City Conference Centre, Century City" },
                    new Address { Street = "Tech Hub", Suburb = "Cape Town CBD", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9245, Longitude = 18.4248, FormattedAddress = "Tech Hub, Cape Town CBD" },
                    new Address { Street = "Rondebosch Park", Suburb = "Rondebosch", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9678, Longitude = 18.4762, FormattedAddress = "Rondebosch Park, Rondebosch" },
                    new Address { Street = "Constantia Wine Route", Suburb = "Constantia", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -34.0259, Longitude = 18.4246, FormattedAddress = "Constantia Wine Route, Constantia" },
                    new Address { Street = "Beer Hall", Suburb = "Cape Town CBD", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9247, Longitude = 18.4243, FormattedAddress = "Beer Hall, Cape Town CBD" },
                    new Address { Street = "Bloubergstrand", Suburb = "Blouberg", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.8236, Longitude = 18.4786, FormattedAddress = "Bloubergstrand, Blouberg" },
                    new Address { Street = "Labia Theatre", Suburb = "Gardens", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9333, Longitude = 18.4097, FormattedAddress = "Labia Theatre, Gardens" },
                    new Address { Street = "Bo-Kaap Museum", Suburb = "Bo-Kaap", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9198, Longitude = 18.4134, FormattedAddress = "Bo-Kaap Museum, Bo-Kaap" },
                    new Address { Street = "University of Cape Town", Suburb = "Rondebosch", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9570, Longitude = 18.4600, FormattedAddress = "University of Cape Town, Rondebosch" },
                    new Address { Street = "Table Mountain National Park", Suburb = "Table Mountain", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9628, Longitude = 18.4098, FormattedAddress = "Table Mountain National Park, Table Mountain" }

                };

                _dbContext.Addresses.AddRange(addresses);
                await _dbContext.SaveChangesAsync();

                // --- EVENT SEEDING ---
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
    }
}