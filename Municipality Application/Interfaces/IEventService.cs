using Municipality_Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Municipality_Application.Interfaces
{
    public interface IEventService
    {
        /// <summary>
        /// Retrieves events filtered by optional search keyword, category, date, or location.
        /// </summary>
        Task<IEnumerable<Event>> GetEventsAsync(string? search, string? category, DateTime? date, double? latitude, double? longitude);

        /// <summary>
        /// Returns recommended events based on user search behavior or category similarity.
        /// </summary>
        Task<IEnumerable<Event>> GetRecommendationsAsync(string? search, string? category, DateTime? date);

        /// <summary>
        /// Gets the set of unique event categories.
        /// </summary>
        Task<HashSet<string>> GetCategoriesAsync();
    }
}