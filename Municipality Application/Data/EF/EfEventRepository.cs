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
        /// Adds a new event to the database.
        /// </summary>
        /// <param name="ev">The event to add.</param>
        /// <returns>The added <see cref="Event"/> entity.</returns>
        public async Task<Event> AddEventAsync(Event ev)
        {
            _dbContext.Events.Add(ev);
            await _dbContext.SaveChangesAsync();
            return ev;
        }

        /// <summary>
        /// Retrieves an event by its unique identifier.
        /// </summary>
        /// <param name="id">The event's unique identifier.</param>
        /// <returns>The <see cref="Event"/> entity if found; otherwise, null.</returns>
        public async Task<Event?> GetEventByIdAsync(int id)
        {
            return await _dbContext.Events.FindAsync(id);
        }

        /// <summary>
        /// Retrieves all events from the database.
        /// </summary>
        /// <returns>A list of all <see cref="Event"/> entities.</returns>
        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            return await _dbContext.Events.ToListAsync();
        }

        /// <summary>
        /// Updates an existing event in the database.
        /// </summary>
        /// <param name="ev">The event with updated data.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public async Task<bool> UpdateEventAsync(Event ev)
        {
            var existing = await _dbContext.Events.FindAsync(ev.Id);
            if (existing == null)
                return false;

            _dbContext.Entry(existing).CurrentValues.SetValues(ev);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Deletes an event from the database.
        /// </summary>
        /// <param name="id">The event's unique identifier.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        public async Task<bool> DeleteEventAsync(int id)
        {
            var ev = await _dbContext.Events.FindAsync(id);
            if (ev == null)
                return false;

            _dbContext.Events.Remove(ev);
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
        /// Seeds the database with default events if none exist.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SeedDefaultEventsAsync()
        {
            if (!await _dbContext.Events.AnyAsync())
            {
                var sampleEvents = new List<Event>
                {
                    new Event { Title = "Water Maintenance", Description = "Scheduled water pipe maintenance.", Category = "Maintenance", Date = DateTime.Today.AddDays(1), Address = "123 Main Street, Cape Town", Location = "Cape Town CBD", Latitude = -33.9249, Longitude = 18.4241, Priority = 2 },
                    new Event { Title = "Road Maintenance", Description = "Scheduled road repair.", Category = "Maintenance", Date = DateTime.Today.AddDays(21), Address = "456 Main Road, Cape Town", Location = "Cape Town CBD", Latitude = -33.9250, Longitude = 18.4250, Priority = 1 },
                    new Event { Title = "Community Meeting", Description = "Monthly community meeting.", Category = "Meeting", Date = DateTime.Today.AddDays(3), Address = "Town Hall, Cape Town", Location = "Cape Town CBD", Latitude = -33.9258, Longitude = 18.4232, Priority = 1 },
                    new Event { Title = "Neighborhood Meeting", Description = "Neighborhood watch meeting.", Category = "Meeting", Date = DateTime.Today.AddDays(22), Address = "Community Center, Cape Town", Location = "Cape Town CBD", Latitude = -33.9260, Longitude = 18.4235, Priority = 2 },
                    new Event { Title = "Festival", Description = "Annual city festival.", Category = "Festival", Date = DateTime.Today.AddDays(7), Address = "Central Park, Cape Town", Location = "Cape Town CBD", Latitude = -33.9270, Longitude = 18.4220, Priority = 3 },
                    new Event { Title = "Spring Festival", Description = "Spring celebration festival.", Category = "Festival", Date = DateTime.Today.AddDays(23), Address = "Botanical Gardens, Cape Town", Location = "Cape Town CBD", Latitude = -33.9275, Longitude = 18.4225, Priority = 2 },
                    new Event { Title = "Sports Day", Description = "Local sports event.", Category = "Sports", Date = DateTime.Today.AddDays(2), Address = "Green Point Stadium, Cape Town", Location = "Green Point", Latitude = -33.9066, Longitude = 18.4114, Priority = 2 },
                    new Event { Title = "Football Match", Description = "Local football match.", Category = "Sports", Date = DateTime.Today.AddDays(24), Address = "Green Point Stadium, Cape Town", Location = "Green Point", Latitude = -33.9067, Longitude = 18.4115, Priority = 1 },
                    new Event { Title = "Art Exhibition", Description = "Contemporary art display.", Category = "Art", Date = DateTime.Today.AddDays(5), Address = "Zeitz MOCAA, Cape Town", Location = "V&A Waterfront", Latitude = -33.9079, Longitude = 18.4206, Priority = 1 },
                    new Event { Title = "Craft Workshop", Description = "Handmade crafts workshop.", Category = "Art", Date = DateTime.Today.AddDays(19), Address = "Fish Hoek Community Centre, Fish Hoek", Location = "Fish Hoek", Latitude = -34.1277, Longitude = 18.4326, Priority = 1 },
                    new Event { Title = "Food Market", Description = "Weekly food market.", Category = "Market", Date = DateTime.Today.AddDays(4), Address = "Old Biscuit Mill, Woodstock", Location = "Woodstock", Latitude = -33.9281, Longitude = 18.4489, Priority = 2 },
                    new Event { Title = "Farmers Market", Description = "Fresh produce market.", Category = "Market", Date = DateTime.Today.AddDays(11), Address = "Oranjezicht City Farm, Oranjezicht", Location = "Oranjezicht", Latitude = -33.9366, Longitude = 18.4137, Priority = 2 },
                    new Event { Title = "Book Fair", Description = "Annual book fair.", Category = "Fair", Date = DateTime.Today.AddDays(6), Address = "Claremont Library, Claremont", Location = "Claremont", Latitude = -33.9781, Longitude = 18.4655, Priority = 1 },
                    new Event { Title = "Science Fair", Description = "School science fair.", Category = "Fair", Date = DateTime.Today.AddDays(25), Address = "Pinelands High School, Pinelands", Location = "Pinelands", Latitude = -33.9362, Longitude = 18.5057, Priority = 2 },
                    new Event { Title = "Music Concert", Description = "Live music event.", Category = "Music", Date = DateTime.Today.AddDays(8), Address = "Kirstenbosch Gardens, Newlands", Location = "Newlands", Latitude = -33.9881, Longitude = 18.4321, Priority = 3 },
                    new Event { Title = "Jazz Night", Description = "Live jazz concert.", Category = "Music", Date = DateTime.Today.AddDays(26), Address = "Jazz Club, Cape Town", Location = "Cape Town CBD", Latitude = -33.9240, Longitude = 18.4245, Priority = 2 },
                    new Event { Title = "Charity Run", Description = "5km charity run.", Category = "Charity", Date = DateTime.Today.AddDays(9), Address = "Sea Point Promenade, Sea Point", Location = "Sea Point", Latitude = -33.9121, Longitude = 18.3872, Priority = 2 },
                    new Event { Title = "Beach Cleanup", Description = "Community beach cleanup.", Category = "Charity", Date = DateTime.Today.AddDays(14), Address = "Muizenberg Beach, Muizenberg", Location = "Muizenberg", Latitude = -34.1052, Longitude = 18.4693, Priority = 2 },
                    new Event { Title = "Tech Meetup", Description = "Monthly tech meetup.", Category = "Tech", Date = DateTime.Today.AddDays(10), Address = "Century City Conference Centre, Century City", Location = "Century City", Latitude = -33.8925, Longitude = 18.5113, Priority = 1 },
                    new Event { Title = "Startup Pitch", Description = "Startup pitch event.", Category = "Tech", Date = DateTime.Today.AddDays(27), Address = "Tech Hub, Cape Town", Location = "Cape Town CBD", Latitude = -33.9245, Longitude = 18.4248, Priority = 2 },
                    new Event { Title = "Yoga in the Park", Description = "Outdoor yoga session.", Category = "Health", Date = DateTime.Today.AddDays(12), Address = "Rondebosch Park, Rondebosch", Location = "Rondebosch", Latitude = -33.9678, Longitude = 18.4762, Priority = 1 },
                    new Event { Title = "Health Expo", Description = "Community health expo.", Category = "Health", Date = DateTime.Today.AddDays(28), Address = "Community Center, Cape Town", Location = "Cape Town CBD", Latitude = -33.9252, Longitude = 18.4239, Priority = 2 },
                    new Event { Title = "Wine Tasting", Description = "Local wine tasting event.", Category = "Food & Drink", Date = DateTime.Today.AddDays(13), Address = "Constantia Wine Route, Constantia", Location = "Constantia", Latitude = -34.0259, Longitude = 18.4246, Priority = 3 },
                    new Event { Title = "Beer Festival", Description = "Craft beer festival.", Category = "Food & Drink", Date = DateTime.Today.AddDays(29), Address = "Beer Hall, Cape Town", Location = "Cape Town CBD", Latitude = -33.9247, Longitude = 18.4243, Priority = 2 },
                    new Event { Title = "Cycling Race", Description = "Annual cycling race.", Category = "Sports", Date = DateTime.Today.AddDays(15), Address = "Bloubergstrand, Blouberg", Location = "Blouberg", Latitude = -33.8236, Longitude = 18.4786, Priority = 1 },
                    new Event { Title = "Marathon", Description = "City marathon event.", Category = "Sports", Date = DateTime.Today.AddDays(30), Address = "City Center, Cape Town", Location = "Cape Town CBD", Latitude = -33.9248, Longitude = 18.4242, Priority = 2 },
                    new Event { Title = "Film Screening", Description = "Outdoor film screening.", Category = "Film", Date = DateTime.Today.AddDays(16), Address = "Labia Theatre, Gardens", Location = "Gardens", Latitude = -33.9333, Longitude = 18.4097, Priority = 2 },
                    new Event { Title = "Documentary Night", Description = "Documentary film screening.", Category = "Film", Date = DateTime.Today.AddDays(31), Address = "Labia Theatre, Gardens", Location = "Gardens", Latitude = -33.9334, Longitude = 18.4098, Priority = 1 },
                    new Event { Title = "Heritage Walk", Description = "Guided heritage walk.", Category = "Culture", Date = DateTime.Today.AddDays(17), Address = "Bo-Kaap Museum, Bo-Kaap", Location = "Bo-Kaap", Latitude = -33.9198, Longitude = 18.4134, Priority = 1 },
                    new Event { Title = "Cultural Parade", Description = "Annual cultural parade.", Category = "Culture", Date = DateTime.Today.AddDays(32), Address = "City Center, Cape Town", Location = "Cape Town CBD", Latitude = -33.9246, Longitude = 18.4244, Priority = 2 },
                    new Event { Title = "Science Expo", Description = "School science expo.", Category = "Education", Date = DateTime.Today.AddDays(18), Address = "Pinelands High School, Pinelands", Location = "Pinelands", Latitude = -33.9361, Longitude = 18.5056, Priority = 2 },
                    new Event { Title = "Education Seminar", Description = "Education improvement seminar.", Category = "Education", Date = DateTime.Today.AddDays(33), Address = "University of Cape Town", Location = "Rondebosch", Latitude = -33.9570, Longitude = 18.4600, Priority = 1 },
                    new Event { Title = "Nature Hike", Description = "Guided nature hike.", Category = "Nature", Date = DateTime.Today.AddDays(20), Address = "Table Mountain National Park, Cape Town", Location = "Table Mountain", Latitude = -33.9628, Longitude = 18.4098, Priority = 3 },
                    new Event { Title = "Bird Watching", Description = "Bird watching event.", Category = "Nature", Date = DateTime.Today.AddDays(34), Address = "Table Mountain National Park, Cape Town", Location = "Table Mountain", Latitude = -33.9630, Longitude = 18.4100, Priority = 2 }
                };

                _dbContext.Events.AddRange(sampleEvents);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}