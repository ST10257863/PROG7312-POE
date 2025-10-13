using Municipality_Application.Models;
using Municipality_Application.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Municipality_Application.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;

        // Data structures for organizing and retrieving event information
        private SortedDictionary<DateTime, List<Event>> _eventsByDate = new();
        private HashSet<string> _categories = new();
        private Dictionary<string, List<Event>> _eventsByKeyword = new();
        private PriorityQueue<Event, int> _priorityQueue = new();
        private Dictionary<string, int> _searchFrequency = new();
        private Stack<Event> _eventStack = new();
        private Queue<Event> _eventQueue = new();

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        // Helper to load and organize events into data structures
        private async Task OrganizeEventsAsync()
        {
            var events = await _eventRepository.GetAllEventsAsync();

            // Clear previous data
            _eventsByDate = new();
            _categories = new();
            _eventsByKeyword = new();
            _priorityQueue = new();
            _eventStack = new();
            _eventQueue = new();

            foreach (var ev in events)
            {
                // SortedDictionary by Date
                if (!_eventsByDate.ContainsKey(ev.Date))
                    _eventsByDate[ev.Date] = new List<Event>();
                _eventsByDate[ev.Date].Add(ev);

                // HashSet for unique categories
                _categories.Add(ev.Category);

                // Dictionary for keyword search
                foreach (var word in ev.Title.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    var key = word.ToLower();
                    if (!_eventsByKeyword.ContainsKey(key))
                        _eventsByKeyword[key] = new List<Event>();
                    _eventsByKeyword[key].Add(ev);
                }

                // PriorityQueue for recommendations
                _priorityQueue.Enqueue(ev, ev.Priority);

                // Stack and Queue for demonstration
                _eventStack.Push(ev);
                _eventQueue.Enqueue(ev);
            }
        }

        public async Task<IEnumerable<Event>> GetEventsAsync(string? search, string? category, DateTime? date, double? latitude, double? longitude)
        {
            await OrganizeEventsAsync();
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
                await _eventRepository.IncrementSearchFrequencyAsync(key);
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

        public async Task<IEnumerable<Event>> GetRecommendationsAsync(string? search, string? category, DateTime? date)
        {
            await OrganizeEventsAsync();

            var searchFrequency = await _eventRepository.GetSearchFrequencyAsync();
            var topSearch = searchFrequency.OrderByDescending(kv => kv.Value).FirstOrDefault().Key;
            if (!string.IsNullOrEmpty(topSearch) && _eventsByKeyword.ContainsKey(topSearch))
                return _eventsByKeyword[topSearch].Take(3);

            if (!string.IsNullOrWhiteSpace(category))
                return _eventsByDate.Values.SelectMany(e => e).Where(ev => ev.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).Take(3);

            return _priorityQueue.UnorderedItems.Select(x => x.Element).OrderBy(e => e.Date).Take(3);
        }

        public async Task<HashSet<string>> GetCategoriesAsync()
        {
            await OrganizeEventsAsync();
            return _categories;
        }

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