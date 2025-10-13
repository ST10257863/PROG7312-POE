using Municipality_Application.Interfaces;
using Municipality_Application.Models;
using System.Collections.Concurrent;

namespace Municipality_Application.Data
{
    public class InMemoryEventRepository : IEventRepository
    {
        private readonly ConcurrentDictionary<int, Event> _events = new();
        private int _nextId = 1;
        private readonly ConcurrentDictionary<string, int> _searchFrequency = new();

        public InMemoryEventRepository()
        {
            // Optionally seed on construction, or call SeedDefaultEventsAsync externally
        }

        public async Task SeedDefaultEventsAsync()
        {
            // Clear existing events to avoid duplicates
            _events.Clear();
            _nextId = 1;

            var sampleEvents = new List<Event>
            {
                new Event { Id = _nextId++, Title = "Water Maintenance", Description = "Scheduled water pipe maintenance.", Category = "Maintenance", Date = DateTime.Today.AddDays(1), Address = "123 Main Street, Cape Town", Location = "Cape Town CBD", Latitude = -33.9249, Longitude = 18.4241, Priority = 2 },
                new Event { Id = _nextId++, Title = "Road Maintenance", Description = "Scheduled road repair.", Category = "Maintenance", Date = DateTime.Today.AddDays(21), Address = "456 Main Road, Cape Town", Location = "Cape Town CBD", Latitude = -33.9250, Longitude = 18.4250, Priority = 1 },
                new Event { Id = _nextId++, Title = "Community Meeting", Description = "Monthly community meeting.", Category = "Meeting", Date = DateTime.Today.AddDays(3), Address = "Town Hall, Cape Town", Location = "Cape Town CBD", Latitude = -33.9258, Longitude = 18.4232, Priority = 1 },
                new Event { Id = _nextId++, Title = "Neighborhood Meeting", Description = "Neighborhood watch meeting.", Category = "Meeting", Date = DateTime.Today.AddDays(22), Address = "Community Center, Cape Town", Location = "Cape Town CBD", Latitude = -33.9260, Longitude = 18.4235, Priority = 2 },
                new Event { Id = _nextId++, Title = "Festival", Description = "Annual city festival.", Category = "Festival", Date = DateTime.Today.AddDays(7), Address = "Central Park, Cape Town", Location = "Cape Town CBD", Latitude = -33.9270, Longitude = 18.4220, Priority = 3 },
                new Event { Id = _nextId++, Title = "Spring Festival", Description = "Spring celebration festival.", Category = "Festival", Date = DateTime.Today.AddDays(23), Address = "Botanical Gardens, Cape Town", Location = "Cape Town CBD", Latitude = -33.9275, Longitude = 18.4225, Priority = 2 },
                new Event { Id = _nextId++, Title = "Sports Day", Description = "Local sports event.", Category = "Sports", Date = DateTime.Today.AddDays(2), Address = "Green Point Stadium, Cape Town", Location = "Green Point", Latitude = -33.9066, Longitude = 18.4114, Priority = 2 },
                new Event { Id = _nextId++, Title = "Football Match", Description = "Local football match.", Category = "Sports", Date = DateTime.Today.AddDays(24), Address = "Green Point Stadium, Cape Town", Location = "Green Point", Latitude = -33.9067, Longitude = 18.4115, Priority = 1 },
                new Event { Id = _nextId++, Title = "Art Exhibition", Description = "Contemporary art display.", Category = "Art", Date = DateTime.Today.AddDays(5), Address = "Zeitz MOCAA, Cape Town", Location = "V&A Waterfront", Latitude = -33.9079, Longitude = 18.4206, Priority = 1 },
                new Event { Id = _nextId++, Title = "Craft Workshop", Description = "Handmade crafts workshop.", Category = "Art", Date = DateTime.Today.AddDays(19), Address = "Fish Hoek Community Centre, Fish Hoek", Location = "Fish Hoek", Latitude = -34.1277, Longitude = 18.4326, Priority = 1 },
                new Event { Id = _nextId++, Title = "Food Market", Description = "Weekly food market.", Category = "Market", Date = DateTime.Today.AddDays(4), Address = "Old Biscuit Mill, Woodstock", Location = "Woodstock", Latitude = -33.9281, Longitude = 18.4489, Priority = 2 },
                new Event { Id = _nextId++, Title = "Farmers Market", Description = "Fresh produce market.", Category = "Market", Date = DateTime.Today.AddDays(11), Address = "Oranjezicht City Farm, Oranjezicht", Location = "Oranjezicht", Latitude = -33.9366, Longitude = 18.4137, Priority = 2 },
                new Event { Id = _nextId++, Title = "Book Fair", Description = "Annual book fair.", Category = "Fair", Date = DateTime.Today.AddDays(6), Address = "Claremont Library, Claremont", Location = "Claremont", Latitude = -33.9781, Longitude = 18.4655, Priority = 1 },
                new Event { Id = _nextId++, Title = "Science Fair", Description = "School science fair.", Category = "Fair", Date = DateTime.Today.AddDays(25), Address = "Pinelands High School, Pinelands", Location = "Pinelands", Latitude = -33.9362, Longitude = 18.5057, Priority = 2 },
                new Event { Id = _nextId++, Title = "Music Concert", Description = "Live music event.", Category = "Music", Date = DateTime.Today.AddDays(8), Address = "Kirstenbosch Gardens, Newlands", Location = "Newlands", Latitude = -33.9881, Longitude = 18.4321, Priority = 3 },
                new Event { Id = _nextId++, Title = "Jazz Night", Description = "Live jazz concert.", Category = "Music", Date = DateTime.Today.AddDays(26), Address = "Jazz Club, Cape Town", Location = "Cape Town CBD", Latitude = -33.9240, Longitude = 18.4245, Priority = 2 },
                new Event { Id = _nextId++, Title = "Charity Run", Description = "5km charity run.", Category = "Charity", Date = DateTime.Today.AddDays(9), Address = "Sea Point Promenade, Sea Point", Location = "Sea Point", Latitude = -33.9121, Longitude = 18.3872, Priority = 2 },
                new Event { Id = _nextId++, Title = "Beach Cleanup", Description = "Community beach cleanup.", Category = "Charity", Date = DateTime.Today.AddDays(14), Address = "Muizenberg Beach, Muizenberg", Location = "Muizenberg", Latitude = -34.1052, Longitude = 18.4693, Priority = 2 },
                new Event { Id = _nextId++, Title = "Tech Meetup", Description = "Monthly tech meetup.", Category = "Tech", Date = DateTime.Today.AddDays(10), Address = "Century City Conference Centre, Century City", Location = "Century City", Latitude = -33.8925, Longitude = 18.5113, Priority = 1 },
                new Event { Id = _nextId++, Title = "Startup Pitch", Description = "Startup pitch event.", Category = "Tech", Date = DateTime.Today.AddDays(27), Address = "Tech Hub, Cape Town", Location = "Cape Town CBD", Latitude = -33.9245, Longitude = 18.4248, Priority = 2 },
                new Event { Id = _nextId++, Title = "Yoga in the Park", Description = "Outdoor yoga session.", Category = "Health", Date = DateTime.Today.AddDays(12), Address = "Rondebosch Park, Rondebosch", Location = "Rondebosch", Latitude = -33.9678, Longitude = 18.4762, Priority = 1 },
                new Event { Id = _nextId++, Title = "Health Expo", Description = "Community health expo.", Category = "Health", Date = DateTime.Today.AddDays(28), Address = "Community Center, Cape Town", Location = "Cape Town CBD", Latitude = -33.9252, Longitude = 18.4239, Priority = 2 },
                new Event { Id = _nextId++, Title = "Wine Tasting", Description = "Local wine tasting event.", Category = "Food & Drink", Date = DateTime.Today.AddDays(13), Address = "Constantia Wine Route, Constantia", Location = "Constantia", Latitude = -34.0259, Longitude = 18.4246, Priority = 3 },
                new Event { Id = _nextId++, Title = "Beer Festival", Description = "Craft beer festival.", Category = "Food & Drink", Date = DateTime.Today.AddDays(29), Address = "Beer Hall, Cape Town", Location = "Cape Town CBD", Latitude = -33.9247, Longitude = 18.4243, Priority = 2 },
                new Event { Id = _nextId++, Title = "Cycling Race", Description = "Annual cycling race.", Category = "Sports", Date = DateTime.Today.AddDays(15), Address = "Bloubergstrand, Blouberg", Location = "Blouberg", Latitude = -33.8236, Longitude = 18.4786, Priority = 1 },
                new Event { Id = _nextId++, Title = "Marathon", Description = "City marathon event.", Category = "Sports", Date = DateTime.Today.AddDays(30), Address = "City Center, Cape Town", Location = "Cape Town CBD", Latitude = -33.9248, Longitude = 18.4242, Priority = 2 },
                new Event { Id = _nextId++, Title = "Film Screening", Description = "Outdoor film screening.", Category = "Film", Date = DateTime.Today.AddDays(16), Address = "Labia Theatre, Gardens", Location = "Gardens", Latitude = -33.9333, Longitude = 18.4097, Priority = 2 },
                new Event { Id = _nextId++, Title = "Documentary Night", Description = "Documentary film screening.", Category = "Film", Date = DateTime.Today.AddDays(31), Address = "Labia Theatre, Gardens", Location = "Gardens", Latitude = -33.9334, Longitude = 18.4098, Priority = 1 },
                new Event { Id = _nextId++, Title = "Heritage Walk", Description = "Guided heritage walk.", Category = "Culture", Date = DateTime.Today.AddDays(17), Address = "Bo-Kaap Museum, Bo-Kaap", Location = "Bo-Kaap", Latitude = -33.9198, Longitude = 18.4134, Priority = 1 },
                new Event { Id = _nextId++, Title = "Cultural Parade", Description = "Annual cultural parade.", Category = "Culture", Date = DateTime.Today.AddDays(32), Address = "City Center, Cape Town", Location = "Cape Town CBD", Latitude = -33.9246, Longitude = 18.4244, Priority = 2 },
                new Event { Id = _nextId++, Title = "Science Expo", Description = "School science expo.", Category = "Education", Date = DateTime.Today.AddDays(18), Address = "Pinelands High School, Pinelands", Location = "Pinelands", Latitude = -33.9361, Longitude = 18.5056, Priority = 2 },
                new Event { Id = _nextId++, Title = "Education Seminar", Description = "Education improvement seminar.", Category = "Education", Date = DateTime.Today.AddDays(33), Address = "University of Cape Town", Location = "Rondebosch", Latitude = -33.9570, Longitude = 18.4600, Priority = 1 },
                new Event { Id = _nextId++, Title = "Nature Hike", Description = "Guided nature hike.", Category = "Nature", Date = DateTime.Today.AddDays(20), Address = "Table Mountain National Park, Cape Town", Location = "Table Mountain", Latitude = -33.9628, Longitude = 18.4098, Priority = 3 },
                new Event { Id = _nextId++, Title = "Bird Watching", Description = "Bird watching event.", Category = "Nature", Date = DateTime.Today.AddDays(34), Address = "Table Mountain National Park, Cape Town", Location = "Table Mountain", Latitude = -33.9630, Longitude = 18.4100, Priority = 2 }
            };

            foreach (var ev in sampleEvents)
            {
                _events[ev.Id] = ev;
            }

            await Task.CompletedTask;
        }

        public Task<Event> AddEventAsync(Event ev)
        {
            ev.Id = _nextId++;
            _events[ev.Id] = ev;
            return Task.FromResult(ev);
        }

        public Task<Event?> GetEventByIdAsync(int id)
        {
            _events.TryGetValue(id, out var ev);
            return Task.FromResult(ev);
        }

        public Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            return Task.FromResult(_events.Values.AsEnumerable());
        }

        public Task<bool> UpdateEventAsync(Event ev)
        {
            if (!_events.ContainsKey(ev.Id))
                return Task.FromResult(false);

            _events[ev.Id] = ev;
            return Task.FromResult(true);
        }

        public Task<bool> DeleteEventAsync(int id)
        {
            return Task.FromResult(_events.TryRemove(id, out _));
        }

        public Task IncrementSearchFrequencyAsync(string searchTerm)
        {
            var key = searchTerm.ToLower();
            _searchFrequency.AddOrUpdate(key, 1, (_, old) => old + 1);
            return Task.CompletedTask;
        }

        public Task<Dictionary<string, int>> GetSearchFrequencyAsync()
        {
            return Task.FromResult(_searchFrequency.ToDictionary(kv => kv.Key, kv => kv.Value));
        }
    }
}