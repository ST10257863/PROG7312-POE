using Municipality_Application.Interfaces;
using Municipality_Application.Models;
using System.Collections.Concurrent;

namespace Municipality_Application.Data.InMemory
{
    /// <summary>
    /// In-memory implementation of <see cref="IEventRepository"/> for managing event and address data during application runtime.
    /// </summary>
    public class InMemoryEventRepository : IEventRepository
    {
        private readonly ConcurrentDictionary<int, Event> _events = new();
        private readonly ConcurrentDictionary<int, Address> _addresses = new();
        private int _nextId = 1;
        private int _nextAddressId = 1;
        private readonly ConcurrentDictionary<string, int> _searchFrequency = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryEventRepository"/> class.
        /// </summary>
        public InMemoryEventRepository()
        {
        }

        /// <summary>
        /// Seeds the in-memory store with default events.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SeedDefaultEventsAsync()
        {
            _events.Clear();
            _addresses.Clear();
            _nextId = 1;
            _nextAddressId = 1;

            // --- Define Addresses ---
            var addresses = new List<Address>
            {
                new Address { Id = _nextAddressId++, Street = "123 Main Street", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9249, Longitude = 18.4241, FormattedAddress = "123 Main Street, Cape Town" },
                new Address { Id = _nextAddressId++, Street = "456 Main Road", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9250, Longitude = 18.4250, FormattedAddress = "456 Main Road, Cape Town" },
                new Address { Id = _nextAddressId++, Street = "Town Hall", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9258, Longitude = 18.4232, FormattedAddress = "Town Hall, Cape Town" },
                new Address { Id = _nextAddressId++, Street = "Community Center", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9260, Longitude = 18.4235, FormattedAddress = "Community Center, Cape Town" },
                new Address { Id = _nextAddressId++, Street = "Central Park", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9270, Longitude = 18.4220, FormattedAddress = "Central Park, Cape Town" },
                new Address { Id = _nextAddressId++, Street = "Botanical Gardens", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9275, Longitude = 18.4225, FormattedAddress = "Botanical Gardens, Cape Town" },
                new Address { Id = _nextAddressId++, Street = "Green Point Stadium", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9066, Longitude = 18.4114, FormattedAddress = "Green Point Stadium, Cape Town" },
                new Address { Id = _nextAddressId++, Street = "Zeitz MOCAA", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9079, Longitude = 18.4206, FormattedAddress = "Zeitz MOCAA, Cape Town" },
                new Address { Id = _nextAddressId++, Street = "Fish Hoek Community Centre", City = "Fish Hoek", Province = "Western Cape", Country = "South Africa", Latitude = -34.1277, Longitude = 18.4326, FormattedAddress = "Fish Hoek Community Centre, Fish Hoek" },
                new Address { Id = _nextAddressId++, Street = "Old Biscuit Mill", City = "Woodstock", Province = "Western Cape", Country = "South Africa", Latitude = -33.9281, Longitude = 18.4489, FormattedAddress = "Old Biscuit Mill, Woodstock" },
                new Address { Id = _nextAddressId++, Street = "Oranjezicht City Farm", City = "Oranjezicht", Province = "Western Cape", Country = "South Africa", Latitude = -33.9366, Longitude = 18.4137, FormattedAddress = "Oranjezicht City Farm, Oranjezicht" },
                new Address { Id = _nextAddressId++, Street = "Claremont Library", City = "Claremont", Province = "Western Cape", Country = "South Africa", Latitude = -33.9781, Longitude = 18.4655, FormattedAddress = "Claremont Library, Claremont" },
                new Address { Id = _nextAddressId++, Street = "Pinelands High School", City = "Pinelands", Province = "Western Cape", Country = "South Africa", Latitude = -33.9362, Longitude = 18.5057, FormattedAddress = "Pinelands High School, Pinelands" },
                new Address { Id = _nextAddressId++, Street = "Kirstenbosch Gardens", City = "Newlands", Province = "Western Cape", Country = "South Africa", Latitude = -33.9881, Longitude = 18.4321, FormattedAddress = "Kirstenbosch Gardens, Newlands" },
                new Address { Id = _nextAddressId++, Street = "Jazz Club", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9240, Longitude = 18.4245, FormattedAddress = "Jazz Club, Cape Town" },
                new Address { Id = _nextAddressId++, Street = "Sea Point Promenade", City = "Sea Point", Province = "Western Cape", Country = "South Africa", Latitude = -33.9121, Longitude = 18.3872, FormattedAddress = "Sea Point Promenade, Sea Point" },
                new Address { Id = _nextAddressId++, Street = "Muizenberg Beach", City = "Muizenberg", Province = "Western Cape", Country = "South Africa", Latitude = -34.1052, Longitude = 18.4693, FormattedAddress = "Muizenberg Beach, Muizenberg" },
                new Address { Id = _nextAddressId++, Street = "Century City Conference Centre", City = "Century City", Province = "Western Cape", Country = "South Africa", Latitude = -33.8925, Longitude = 18.5113, FormattedAddress = "Century City Conference Centre, Century City" },
                new Address { Id = _nextAddressId++, Street = "Tech Hub", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9245, Longitude = 18.4248, FormattedAddress = "Tech Hub, Cape Town" },
                new Address { Id = _nextAddressId++, Street = "Rondebosch Park", City = "Rondebosch", Province = "Western Cape", Country = "South Africa", Latitude = -33.9678, Longitude = 18.4762, FormattedAddress = "Rondebosch Park, Rondebosch" },
                new Address { Id = _nextAddressId++, Street = "Community Center", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9252, Longitude = 18.4239, FormattedAddress = "Community Center, Cape Town" },
                new Address { Id = _nextAddressId++, Street = "Constantia Wine Route", City = "Constantia", Province = "Western Cape", Country = "South Africa", Latitude = -34.0259, Longitude = 18.4246, FormattedAddress = "Constantia Wine Route, Constantia" },
                new Address { Id = _nextAddressId++, Street = "Beer Hall", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9247, Longitude = 18.4243, FormattedAddress = "Beer Hall, Cape Town" },
                new Address { Id = _nextAddressId++, Street = "Bloubergstrand", City = "Blouberg", Province = "Western Cape", Country = "South Africa", Latitude = -33.8236, Longitude = 18.4786, FormattedAddress = "Bloubergstrand, Blouberg" },
                new Address { Id = _nextAddressId++, Street = "City Center", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9248, Longitude = 18.4242, FormattedAddress = "City Center, Cape Town" },
                new Address { Id = _nextAddressId++, Street = "Labia Theatre", City = "Gardens", Province = "Western Cape", Country = "South Africa", Latitude = -33.9333, Longitude = 18.4097, FormattedAddress = "Labia Theatre, Gardens" },
                new Address { Id = _nextAddressId++, Street = "Bo-Kaap Museum", City = "Bo-Kaap", Province = "Western Cape", Country = "South Africa", Latitude = -33.9198, Longitude = 18.4134, FormattedAddress = "Bo-Kaap Museum, Bo-Kaap" },
                new Address { Id = _nextAddressId++, Street = "University of Cape Town", City = "Rondebosch", Province = "Western Cape", Country = "South Africa", Latitude = -33.9570, Longitude = 18.4600, FormattedAddress = "University of Cape Town" },
                new Address { Id = _nextAddressId++, Street = "Table Mountain National Park", City = "Cape Town", Province = "Western Cape", Country = "South Africa", Latitude = -33.9628, Longitude = 18.4098, FormattedAddress = "Table Mountain National Park, Cape Town" }
            };

            // Store addresses in dictionary
            foreach (var address in addresses)
            {
                _addresses[address.Id] = address;
            }

            // --- Define Events ---
            var sampleEvents = new List<Event>
            {
                new Event { Id = _nextId++, Title = "Water Maintenance", Description = "Scheduled water pipe maintenance.", Category = "Maintenance", Date = DateTime.Today.AddDays(1), AddressId = addresses[0].Id, Address = addresses[0], Priority = 2 },
                new Event { Id = _nextId++, Title = "Road Maintenance", Description = "Scheduled road repair.", Category = "Maintenance", Date = DateTime.Today.AddDays(21), AddressId = addresses[1].Id, Address = addresses[1], Priority = 1 },
                new Event { Id = _nextId++, Title = "Community Meeting", Description = "Monthly community meeting.", Category = "Meeting", Date = DateTime.Today.AddDays(3), AddressId = addresses[2].Id, Address = addresses[2], Priority = 1 },
                new Event { Id = _nextId++, Title = "Neighborhood Meeting", Description = "Neighborhood watch meeting.", Category = "Meeting", Date = DateTime.Today.AddDays(22), AddressId = addresses[3].Id, Address = addresses[3], Priority = 2 },
                new Event { Id = _nextId++, Title = "Festival", Description = "Annual city festival.", Category = "Festival", Date = DateTime.Today.AddDays(7), AddressId = addresses[4].Id, Address = addresses[4], Priority = 3 },
                new Event { Id = _nextId++, Title = "Spring Festival", Description = "Spring celebration festival.", Category = "Festival", Date = DateTime.Today.AddDays(23), AddressId = addresses[5].Id, Address = addresses[5], Priority = 2 },
                new Event { Id = _nextId++, Title = "Sports Day", Description = "Local sports event.", Category = "Sports", Date = DateTime.Today.AddDays(2), AddressId = addresses[6].Id, Address = addresses[6], Priority = 2 },
                new Event { Id = _nextId++, Title = "Football Match", Description = "Local football match.", Category = "Sports", Date = DateTime.Today.AddDays(24), AddressId = addresses[6].Id, Address = addresses[6], Priority = 1 },
                new Event { Id = _nextId++, Title = "Art Exhibition", Description = "Contemporary art display.", Category = "Art", Date = DateTime.Today.AddDays(5), AddressId = addresses[7].Id, Address = addresses[7], Priority = 1 },
                new Event { Id = _nextId++, Title = "Craft Workshop", Description = "Handmade crafts workshop.", Category = "Art", Date = DateTime.Today.AddDays(19), AddressId = addresses[8].Id, Address = addresses[8], Priority = 1 },
                new Event { Id = _nextId++, Title = "Food Market", Description = "Weekly food market.", Category = "Market", Date = DateTime.Today.AddDays(4), AddressId = addresses[9].Id, Address = addresses[9], Priority = 2 },
                new Event { Id = _nextId++, Title = "Farmers Market", Description = "Fresh produce market.", Category = "Market", Date = DateTime.Today.AddDays(11), AddressId = addresses[10].Id, Address = addresses[10], Priority = 2 },
                new Event { Id = _nextId++, Title = "Book Fair", Description = "Annual book fair.", Category = "Fair", Date = DateTime.Today.AddDays(6), AddressId = addresses[11].Id, Address = addresses[11], Priority = 1 },
                new Event { Id = _nextId++, Title = "Science Fair", Description = "School science fair.", Category = "Fair", Date = DateTime.Today.AddDays(25), AddressId = addresses[12].Id, Address = addresses[12], Priority = 2 },
                new Event { Id = _nextId++, Title = "Music Concert", Description = "Live music event.", Category = "Music", Date = DateTime.Today.AddDays(8), AddressId = addresses[13].Id, Address = addresses[13], Priority = 3 },
                new Event { Id = _nextId++, Title = "Jazz Night", Description = "Live jazz concert.", Category = "Music", Date = DateTime.Today.AddDays(26), AddressId = addresses[14].Id, Address = addresses[14], Priority = 2 },
                new Event { Id = _nextId++, Title = "Charity Run", Description = "5km charity run.", Category = "Charity", Date = DateTime.Today.AddDays(9), AddressId = addresses[15].Id, Address = addresses[15], Priority = 2 },
                new Event { Id = _nextId++, Title = "Beach Cleanup", Description = "Community beach cleanup.", Category = "Charity", Date = DateTime.Today.AddDays(14), AddressId = addresses[16].Id, Address = addresses[16], Priority = 2 },
                new Event { Id = _nextId++, Title = "Tech Meetup", Description = "Monthly tech meetup.", Category = "Tech", Date = DateTime.Today.AddDays(10), AddressId = addresses[17].Id, Address = addresses[17], Priority = 1 },
                new Event { Id = _nextId++, Title = "Startup Pitch", Description = "Startup pitch event.", Category = "Tech", Date = DateTime.Today.AddDays(27), AddressId = addresses[18].Id, Address = addresses[18], Priority = 2 },
                new Event { Id = _nextId++, Title = "Yoga in the Park", Description = "Outdoor yoga session.", Category = "Health", Date = DateTime.Today.AddDays(12), AddressId = addresses[19].Id, Address = addresses[19], Priority = 1 },
                new Event { Id = _nextId++, Title = "Health Expo", Description = "Community health expo.", Category = "Health", Date = DateTime.Today.AddDays(28), AddressId = addresses[20].Id, Address = addresses[20], Priority = 2 },
                new Event { Id = _nextId++, Title = "Wine Tasting", Description = "Local wine tasting event.", Category = "Food & Drink", Date = DateTime.Today.AddDays(13), AddressId = addresses[21].Id, Address = addresses[21], Priority = 3 },
                new Event { Id = _nextId++, Title = "Beer Festival", Description = "Craft beer festival.", Category = "Food & Drink", Date = DateTime.Today.AddDays(29), AddressId = addresses[22].Id, Address = addresses[22], Priority = 2 },
                new Event { Id = _nextId++, Title = "Cycling Race", Description = "Annual cycling race.", Category = "Sports", Date = DateTime.Today.AddDays(15), AddressId = addresses[23].Id, Address = addresses[23], Priority = 1 },
                new Event { Id = _nextId++, Title = "Marathon", Description = "City marathon event.", Category = "Sports", Date = DateTime.Today.AddDays(30), AddressId = addresses[24].Id, Address = addresses[24], Priority = 2 },
                new Event { Id = _nextId++, Title = "Film Screening", Description = "Outdoor film screening.", Category = "Film", Date = DateTime.Today.AddDays(16), AddressId = addresses[25].Id, Address = addresses[25], Priority = 2 },
                new Event { Id = _nextId++, Title = "Documentary Night", Description = "Documentary film screening.", Category = "Film", Date = DateTime.Today.AddDays(31), AddressId = addresses[25].Id, Address = addresses[25], Priority = 1 },
                new Event { Id = _nextId++, Title = "Heritage Walk", Description = "Guided heritage walk.", Category = "Culture", Date = DateTime.Today.AddDays(17), AddressId = addresses[26].Id, Address = addresses[26], Priority = 1 },
                new Event { Id = _nextId++, Title = "Cultural Parade", Description = "Annual cultural parade.", Category = "Culture", Date = DateTime.Today.AddDays(32), AddressId = addresses[24].Id, Address = addresses[24], Priority = 2 },
                new Event { Id = _nextId++, Title = "Science Expo", Description = "School science expo.", Category = "Education", Date = DateTime.Today.AddDays(18), AddressId = addresses[12].Id, Address = addresses[12], Priority = 2 },
                new Event { Id = _nextId++, Title = "Education Seminar", Description = "Education improvement seminar.", Category = "Education", Date = DateTime.Today.AddDays(33), AddressId = addresses[27].Id, Address = addresses[27], Priority = 1 },
                new Event { Id = _nextId++, Title = "Nature Hike", Description = "Guided nature hike.", Category = "Nature", Date = DateTime.Today.AddDays(20), AddressId = addresses[28].Id, Address = addresses[28], Priority = 3 },
                new Event { Id = _nextId++, Title = "Bird Watching", Description = "Bird watching event.", Category = "Nature", Date = DateTime.Today.AddDays(34), AddressId = addresses[28].Id, Address = addresses[28], Priority = 2 },
            };

            foreach (var ev in sampleEvents)
            {
                _events[ev.Id] = ev;
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Adds a new event to the in-memory store.
        /// </summary>
        /// <param name="ev">The event to add.</param>
        /// <returns>The added <see cref="Event"/> entity.</returns>
        public Task<Event> AddEventAsync(Event ev)
        {
            ev.Id = _nextId++;
            // Handle Address
            if (ev.Address != null)
            {
                if (ev.Address.Id == 0)
                    ev.Address.Id = _nextAddressId++;
                _addresses[ev.Address.Id] = ev.Address;
                ev.AddressId = ev.Address.Id;
            }
            _events[ev.Id] = ev;
            return Task.FromResult(ev);
        }

        /// <summary>
        /// Retrieves an event by its unique identifier.
        /// </summary>
        /// <param name="id">The event's unique identifier.</param>
        /// <returns>The <see cref="Event"/> entity if found; otherwise, null.</returns>
        public Task<Event?> GetEventByIdAsync(int id)
        {
            _events.TryGetValue(id, out var ev);
            if (ev != null && ev.AddressId != 0 && _addresses.TryGetValue(ev.AddressId, out var address))
                ev.Address = address;
            return Task.FromResult(ev);
        }

        /// <summary>
        /// Retrieves all events from the in-memory store.
        /// </summary>
        /// <returns>A list of all <see cref="Event"/> entities.</returns>
        public Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            var events = _events.Values.ToList();
            foreach (var ev in events)
            {
                if (ev.AddressId != 0 && _addresses.TryGetValue(ev.AddressId, out var address))
                    ev.Address = address;
            }
            return Task.FromResult(events.AsEnumerable());
        }

        /// <summary>
        /// Updates an existing event in the in-memory store.
        /// </summary>
        /// <param name="ev">The event with updated data.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public Task<bool> UpdateEventAsync(Event ev)
        {
            if (!_events.ContainsKey(ev.Id))
                return Task.FromResult(false);

            // Update Address
            if (ev.Address != null)
            {
                if (ev.Address.Id == 0)
                    ev.Address.Id = _nextAddressId++;
                _addresses[ev.Address.Id] = ev.Address;
                ev.AddressId = ev.Address.Id;
            }
            _events[ev.Id] = ev;
            return Task.FromResult(true);
        }

        /// <summary>
        /// Deletes an event from the in-memory store.
        /// </summary>
        /// <param name="id">The event's unique identifier.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        public Task<bool> DeleteEventAsync(int id)
        {
            var removed = _events.TryRemove(id, out var ev);
            // Optionally remove the address if not referenced by any other event
            if (ev != null && ev.AddressId != 0)
            {
                bool addressInUse = _events.Values.Any(e => e.AddressId == ev.AddressId && e.Id != id);
                if (!addressInUse)
                {
                    _addresses.TryRemove(ev.AddressId, out _);
                }
            }
            return Task.FromResult(removed);
        }

        /// <summary>
        /// Increments the search frequency for a given search term.
        /// </summary>
        /// <param name="searchTerm">The search term to increment.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task IncrementSearchFrequencyAsync(string searchTerm)
        {
            var key = searchTerm.ToLower();
            _searchFrequency.AddOrUpdate(key, 1, (_, old) => old + 1);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Retrieves the search frequency dictionary for all search terms.
        /// </summary>
        /// <returns>A dictionary mapping search terms to their frequency.</returns>
        public Task<Dictionary<string, int>> GetSearchFrequencyAsync()
        {
            return Task.FromResult(_searchFrequency.ToDictionary(kv => kv.Key, kv => kv.Value));
        }
    }
}