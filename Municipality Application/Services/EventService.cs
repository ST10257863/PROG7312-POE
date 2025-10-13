using Municipality_Application.Models;
using Municipality_Application.Interfaces;

namespace Municipality_Application.Services
{
    /// <summary>
    /// Provides higher-level event operations such as filtering, recommendations, and category retrieval.
    /// Organizes event data using various data structures for efficient access and demonstration purposes.
    /// </summary>
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;

        // Data structures for organizing and retrieving event information
        private SortedDictionary<DateTime, List<Event>> _eventsByDate = new();
        private HashSet<string> _categories = new();
        private Dictionary<string, List<Event>> _eventsByKeyword = new();
        private PriorityQueue<Event, int> _priorityQueue = new();
        private Stack<Event> _eventStack = new();
        private Queue<Event> _eventQueue = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="EventService"/> class.
        /// </summary>
        /// <param name="eventRepository">The event repository instance.</param>
        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        /// <summary>
        /// Loads and organizes events into internal data structures for efficient access.
        /// </summary>
        private async Task OrganizeEventsAsync()
        {
            var events = await _eventRepository.GetAllEventsAsync();

            _eventsByDate = new();
            _categories = new();
            _eventsByKeyword = new();
            _priorityQueue = new();
            _eventStack = new();
            _eventQueue = new();

            foreach (var ev in events)
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

                _eventStack.Push(ev);
                _eventQueue.Enqueue(ev);
            }
        }

        /// <summary>
        /// Retrieves events filtered by optional search keyword, category, date, or location.
        /// </summary>
        /// <param name="search">Optional search keyword.</param>
        /// <param name="category">Optional event category.</param>
        /// <param name="date">Optional event date.</param>
        /// <param name="latitude">Optional latitude for location-based filtering.</param>
        /// <param name="longitude">Optional longitude for location-based filtering.</param>
        /// <returns>An enumerable collection of filtered <see cref="Event"/> objects.</returns>
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

        /// <summary>
        /// Returns recommended events based on user search behavior or category similarity.
        /// </summary>
        /// <param name="search">Optional search keyword.</param>
        /// <param name="category">Optional event category.</param>
        /// <param name="date">Optional event date.</param>
        /// <returns>An enumerable collection of recommended <see cref="Event"/> objects.</returns>
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

        /// <summary>
        /// Gets the set of unique event categories.
        /// </summary>
        /// <returns>A set of unique category names.</returns>
        public async Task<HashSet<string>> GetCategoriesAsync()
        {
            await OrganizeEventsAsync();
            return _categories;
        }

        /// <summary>
        /// Increments the search frequency for a given search term.
        /// </summary>
        /// <param name="searchTerm">The search term to increment.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task IncrementSearchFrequencyAsync(string searchTerm)
        {
            await _eventRepository.IncrementSearchFrequencyAsync(searchTerm);
        }

        /// <summary>
        /// Retrieves the search frequency dictionary for all search terms.
        /// </summary>
        /// <returns>A dictionary mapping search terms to their frequency.</returns>
        public async Task<Dictionary<string, int>> GetSearchFrequencyAsync()
        {
            return await _eventRepository.GetSearchFrequencyAsync();
        }

        /// <summary>
        /// Calculates the distance in kilometers between two geographic coordinates.
        /// </summary>
        /// <param name="lat1">Latitude of the first point.</param>
        /// <param name="lon1">Longitude of the first point.</param>
        /// <param name="lat2">Latitude of the second point.</param>
        /// <param name="lon2">Longitude of the second point.</param>
        /// <returns>The distance in kilometers.</returns>
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