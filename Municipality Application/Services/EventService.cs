using Municipality_Application.Models;
using Municipality_Application.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Municipality_Application.Services
{
    public class EventService : IEventService
    {
        private readonly SortedDictionary<DateTime, List<Event>> _eventsByDate = new();
        private readonly HashSet<string> _categories = new();
        private readonly Dictionary<string, List<Event>> _eventsByKeyword = new();
        private readonly PriorityQueue<Event, int> _priorityQueue = new();
        private readonly Dictionary<string, int> _searchFrequency = new();

        public EventService()
        {
            // Seed with sample events (replace with DB integration as needed)
            var sampleEvents = new List<Event>
            {
                new Event { Id = 1, Title = "Water Maintenance", Description = "Scheduled water pipe maintenance.", Category = "Maintenance", Date = DateTime.Today.AddDays(1), Address = "123 Main Street, Cape Town", Location = "Cape Town CBD", Latitude = -33.9249, Longitude = 18.4241, Priority = 2 },
                new Event { Id = 2, Title = "Road Maintenance", Description = "Scheduled road repair.", Category = "Maintenance", Date = DateTime.Today.AddDays(21), Address = "456 Main Road, Cape Town", Location = "Cape Town CBD", Latitude = -33.9250, Longitude = 18.4250, Priority = 1 },

                new Event { Id = 3, Title = "Community Meeting", Description = "Monthly community meeting.", Category = "Meeting", Date = DateTime.Today.AddDays(3), Address = "Town Hall, Cape Town", Location = "Cape Town CBD", Latitude = -33.9258, Longitude = 18.4232, Priority = 1 },
                new Event { Id = 4, Title = "Neighborhood Meeting", Description = "Neighborhood watch meeting.", Category = "Meeting", Date = DateTime.Today.AddDays(22), Address = "Community Center, Cape Town", Location = "Cape Town CBD", Latitude = -33.9260, Longitude = 18.4235, Priority = 2 },

                new Event { Id = 5, Title = "Festival", Description = "Annual city festival.", Category = "Festival", Date = DateTime.Today.AddDays(7), Address = "Central Park, Cape Town", Location = "Cape Town CBD", Latitude = -33.9270, Longitude = 18.4220, Priority = 3 },
                new Event { Id = 6, Title = "Spring Festival", Description = "Spring celebration festival.", Category = "Festival", Date = DateTime.Today.AddDays(23), Address = "Botanical Gardens, Cape Town", Location = "Cape Town CBD", Latitude = -33.9275, Longitude = 18.4225, Priority = 2 },

                new Event { Id = 7, Title = "Sports Day", Description = "Local sports event.", Category = "Sports", Date = DateTime.Today.AddDays(2), Address = "Green Point Stadium, Cape Town", Location = "Green Point", Latitude = -33.9066, Longitude = 18.4114, Priority = 2 },
                new Event { Id = 8, Title = "Football Match", Description = "Local football match.", Category = "Sports", Date = DateTime.Today.AddDays(24), Address = "Green Point Stadium, Cape Town", Location = "Green Point", Latitude = -33.9067, Longitude = 18.4115, Priority = 1 },

                new Event { Id = 9, Title = "Art Exhibition", Description = "Contemporary art display.", Category = "Art", Date = DateTime.Today.AddDays(5), Address = "Zeitz MOCAA, Cape Town", Location = "V&A Waterfront", Latitude = -33.9079, Longitude = 18.4206, Priority = 1 },
                new Event { Id = 10, Title = "Craft Workshop", Description = "Handmade crafts workshop.", Category = "Art", Date = DateTime.Today.AddDays(19), Address = "Fish Hoek Community Centre, Fish Hoek", Location = "Fish Hoek", Latitude = -34.1277, Longitude = 18.4326, Priority = 1 },

                new Event { Id = 11, Title = "Food Market", Description = "Weekly food market.", Category = "Market", Date = DateTime.Today.AddDays(4), Address = "Old Biscuit Mill, Woodstock", Location = "Woodstock", Latitude = -33.9281, Longitude = 18.4489, Priority = 2 },
                new Event { Id = 12, Title = "Farmers Market", Description = "Fresh produce market.", Category = "Market", Date = DateTime.Today.AddDays(11), Address = "Oranjezicht City Farm, Oranjezicht", Location = "Oranjezicht", Latitude = -33.9366, Longitude = 18.4137, Priority = 2 },

                new Event { Id = 13, Title = "Book Fair", Description = "Annual book fair.", Category = "Fair", Date = DateTime.Today.AddDays(6), Address = "Claremont Library, Claremont", Location = "Claremont", Latitude = -33.9781, Longitude = 18.4655, Priority = 1 },
                new Event { Id = 14, Title = "Science Fair", Description = "School science fair.", Category = "Fair", Date = DateTime.Today.AddDays(25), Address = "Pinelands High School, Pinelands", Location = "Pinelands", Latitude = -33.9362, Longitude = 18.5057, Priority = 2 },

                new Event { Id = 15, Title = "Music Concert", Description = "Live music event.", Category = "Music", Date = DateTime.Today.AddDays(8), Address = "Kirstenbosch Gardens, Newlands", Location = "Newlands", Latitude = -33.9881, Longitude = 18.4321, Priority = 3 },
                new Event { Id = 16, Title = "Jazz Night", Description = "Live jazz concert.", Category = "Music", Date = DateTime.Today.AddDays(26), Address = "Jazz Club, Cape Town", Location = "Cape Town CBD", Latitude = -33.9240, Longitude = 18.4245, Priority = 2 },

                new Event { Id = 17, Title = "Charity Run", Description = "5km charity run.", Category = "Charity", Date = DateTime.Today.AddDays(9), Address = "Sea Point Promenade, Sea Point", Location = "Sea Point", Latitude = -33.9121, Longitude = 18.3872, Priority = 2 },
                new Event { Id = 18, Title = "Beach Cleanup", Description = "Community beach cleanup.", Category = "Charity", Date = DateTime.Today.AddDays(14), Address = "Muizenberg Beach, Muizenberg", Location = "Muizenberg", Latitude = -34.1052, Longitude = 18.4693, Priority = 2 },

                new Event { Id = 19, Title = "Tech Meetup", Description = "Monthly tech meetup.", Category = "Tech", Date = DateTime.Today.AddDays(10), Address = "Century City Conference Centre, Century City", Location = "Century City", Latitude = -33.8925, Longitude = 18.5113, Priority = 1 },
                new Event { Id = 20, Title = "Startup Pitch", Description = "Startup pitch event.", Category = "Tech", Date = DateTime.Today.AddDays(27), Address = "Tech Hub, Cape Town", Location = "Cape Town CBD", Latitude = -33.9245, Longitude = 18.4248, Priority = 2 },

                new Event { Id = 21, Title = "Yoga in the Park", Description = "Outdoor yoga session.", Category = "Health", Date = DateTime.Today.AddDays(12), Address = "Rondebosch Park, Rondebosch", Location = "Rondebosch", Latitude = -33.9678, Longitude = 18.4762, Priority = 1 },
                new Event { Id = 22, Title = "Health Expo", Description = "Community health expo.", Category = "Health", Date = DateTime.Today.AddDays(28), Address = "Community Center, Cape Town", Location = "Cape Town CBD", Latitude = -33.9252, Longitude = 18.4239, Priority = 2 },

                new Event { Id = 23, Title = "Wine Tasting", Description = "Local wine tasting event.", Category = "Food & Drink", Date = DateTime.Today.AddDays(13), Address = "Constantia Wine Route, Constantia", Location = "Constantia", Latitude = -34.0259, Longitude = 18.4246, Priority = 3 },
                new Event { Id = 24, Title = "Beer Festival", Description = "Craft beer festival.", Category = "Food & Drink", Date = DateTime.Today.AddDays(29), Address = "Beer Hall, Cape Town", Location = "Cape Town CBD", Latitude = -33.9247, Longitude = 18.4243, Priority = 2 },

                new Event { Id = 25, Title = "Cycling Race", Description = "Annual cycling race.", Category = "Sports", Date = DateTime.Today.AddDays(15), Address = "Bloubergstrand, Blouberg", Location = "Blouberg", Latitude = -33.8236, Longitude = 18.4786, Priority = 1 },
                new Event { Id = 26, Title = "Marathon", Description = "City marathon event.", Category = "Sports", Date = DateTime.Today.AddDays(30), Address = "City Center, Cape Town", Location = "Cape Town CBD", Latitude = -33.9248, Longitude = 18.4242, Priority = 2 },

                new Event { Id = 27, Title = "Film Screening", Description = "Outdoor film screening.", Category = "Film", Date = DateTime.Today.AddDays(16), Address = "Labia Theatre, Gardens", Location = "Gardens", Latitude = -33.9333, Longitude = 18.4097, Priority = 2 },
                new Event { Id = 28, Title = "Documentary Night", Description = "Documentary film screening.", Category = "Film", Date = DateTime.Today.AddDays(31), Address = "Labia Theatre, Gardens", Location = "Gardens", Latitude = -33.9334, Longitude = 18.4098, Priority = 1 },

                new Event { Id = 29, Title = "Heritage Walk", Description = "Guided heritage walk.", Category = "Culture", Date = DateTime.Today.AddDays(17), Address = "Bo-Kaap Museum, Bo-Kaap", Location = "Bo-Kaap", Latitude = -33.9198, Longitude = 18.4134, Priority = 1 },
                new Event { Id = 30, Title = "Cultural Parade", Description = "Annual cultural parade.", Category = "Culture", Date = DateTime.Today.AddDays(32), Address = "City Center, Cape Town", Location = "Cape Town CBD", Latitude = -33.9246, Longitude = 18.4244, Priority = 2 },

                new Event { Id = 31, Title = "Science Expo", Description = "School science expo.", Category = "Education", Date = DateTime.Today.AddDays(18), Address = "Pinelands High School, Pinelands", Location = "Pinelands", Latitude = -33.9361, Longitude = 18.5056, Priority = 2 },
                new Event { Id = 32, Title = "Education Seminar", Description = "Education improvement seminar.", Category = "Education", Date = DateTime.Today.AddDays(33), Address = "University of Cape Town", Location = "Rondebosch", Latitude = -33.9570, Longitude = 18.4600, Priority = 1 },

                new Event { Id = 33, Title = "Nature Hike", Description = "Guided nature hike.", Category = "Nature", Date = DateTime.Today.AddDays(20), Address = "Table Mountain National Park, Cape Town", Location = "Table Mountain", Latitude = -33.9628, Longitude = 18.4098, Priority = 3 },
                new Event { Id = 34, Title = "Bird Watching", Description = "Bird watching event.", Category = "Nature", Date = DateTime.Today.AddDays(34), Address = "Table Mountain National Park, Cape Town", Location = "Table Mountain", Latitude = -33.9630, Longitude = 18.4100, Priority = 2 }
            };

            foreach (var ev in sampleEvents)
            {
                if (!_eventsByDate.ContainsKey(ev.Date))
                    _eventsByDate[ev.Date] = new List<Event>();
                _eventsByDate[ev.Date].Add(ev);

                _categories.Add(ev.Category);

                foreach (var word in ev.Title.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    var key = word.ToLower();
                    if (!_eventsByKeyword.ContainsKey(key))
                        _eventsByKeyword[key] = new List<Event>();
                    _eventsByKeyword[key].Add(ev);
                }

                _priorityQueue.Enqueue(ev, ev.Priority);
            }
        }

        public IEnumerable<Event> GetEvents(string? search, string? category, DateTime? date, double? latitude, double? longitude)
        {
            IEnumerable<Event> result = _eventsByDate.Values.SelectMany(e => e);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var key = search.ToLower();
                result = result.Where(e =>
                    (!string.IsNullOrEmpty(e.Title) && e.Title.ToLower().Contains(key)) ||
                    (!string.IsNullOrEmpty(e.Description) && e.Description.ToLower().Contains(key)) ||
                    (!string.IsNullOrEmpty(e.Category) && e.Category.ToLower().Contains(key)) ||
                    (!string.IsNullOrEmpty(e.Address) && e.Address.ToLower().Contains(key))
                );
                _searchFrequency[key] = _searchFrequency.GetValueOrDefault(key, 0) + 1;
            }

            if (!string.IsNullOrWhiteSpace(category))
                result = result.Where(e => e.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

            if (date.HasValue)
                result = result.Where(e => e.Date.Date == date.Value.Date);

            // Sort by proximity if coordinates are provided
            if (latitude.HasValue && longitude.HasValue)
            {
                result = result
                    .Where(e => e.Latitude.HasValue && e.Longitude.HasValue)
                    .OrderBy(e => GetDistance(latitude.Value, longitude.Value, e.Latitude.Value, e.Longitude.Value));
            }
            else
            {
                result = result.OrderBy(e => e.Date);
            }

            return result;
        }

        public IEnumerable<Event> GetRecommendations(string? search, string? category, DateTime? date)
        {
            var topSearch = _searchFrequency.OrderByDescending(kv => kv.Value).FirstOrDefault().Key;
            if (!string.IsNullOrEmpty(topSearch) && _eventsByKeyword.ContainsKey(topSearch))
                return _eventsByKeyword[topSearch].Take(3);

            if (!string.IsNullOrWhiteSpace(category))
                return _eventsByDate.Values.SelectMany(e => e).Where(ev => ev.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).Take(3);

            return _priorityQueue.UnorderedItems.Select(x => x.Element).OrderBy(e => e.Date).Take(3);
        }

        public HashSet<string> GetCategories() => _categories;

        private static double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth radius in km
            var dLat = Math.PI / 180 * (lat2 - lat1);
            var dLon = Math.PI / 180 * (lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(Math.PI / 180 * lat1) * Math.Cos(Math.PI / 180 * lat2) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }
}