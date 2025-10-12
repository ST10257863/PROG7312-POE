using Municipality_Application.Models;
using Municipality_Application.Interfaces;
using System.Collections.Generic;

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
                new Event { Id = 1, Title = "Water Maintenance", Description = "Scheduled water pipe maintenance.", Category = "Maintenance", Date = DateTime.Today.AddDays(1), Address = "123 Main Street, Cape Town", Location = "Main Street", Latitude = -33.9249, Longitude = 18.4241, Priority = 2 },
                new Event { Id = 2, Title = "Community Meeting", Description = "Monthly community meeting.", Category = "Meeting", Date = DateTime.Today.AddDays(3), Address = "Town Hall, Cape Town", Location = "Town Hall", Latitude = -33.9258, Longitude = 18.4232, Priority = 1 },
                new Event { Id = 3, Title = "Festival", Description = "Annual city festival.", Category = "Festival", Date = DateTime.Today.AddDays(7), Address = "Central Park, Cape Town", Location = "Central Park", Latitude = -33.9270, Longitude = 18.4220, Priority = 3 }
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

        public IEnumerable<Event> GetEvents(string? search, string? category, DateTime? date)
        {
            IEnumerable<Event> result = _eventsByDate.Values.SelectMany(e => e);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var key = search.ToLower();
                if (_eventsByKeyword.ContainsKey(key))
                    result = _eventsByKeyword[key];
                _searchFrequency[key] = _searchFrequency.GetValueOrDefault(key, 0) + 1;
            }

            if (!string.IsNullOrWhiteSpace(category))
                result = result.Where(e => e.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

            if (date.HasValue)
                result = result.Where(e => e.Date.Date == date.Value.Date);

            return result.OrderBy(e => e.Date);
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
    }
}