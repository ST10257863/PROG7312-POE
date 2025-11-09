using Municipality_Application.Models;

namespace Municipality_Application.Interfaces.Service
{
    /// <summary>
    /// Provides higher-level event operations such as filtering, recommendations, and category retrieval.
    /// </summary>
    public interface IEventService
    {
        /// <summary>
        /// Retrieves events filtered by optional search keyword, category, date, or location.
        /// </summary>
        /// <param name="search">Optional search keyword.</param>
        /// <param name="category">Optional event category.</param>
        /// <param name="date">Optional event date.</param>
        /// <param name="latitude">Optional latitude for location-based filtering.</param>
        /// <param name="longitude">Optional longitude for location-based filtering.</param>
        /// <returns>An enumerable collection of filtered <see cref="Event"/> objects.</returns>
        Task<IEnumerable<Event>> GetEventsAsync(string? search, string? category, DateTime? date, double? latitude, double? longitude);

        /// <summary>
        /// Returns recommended events based on user search behavior or category similarity.
        /// </summary>
        /// <param name="search">Optional search keyword.</param>
        /// <param name="category">Optional event category.</param>
        /// <param name="date">Optional event date.</param>
        /// <returns>An enumerable collection of recommended <see cref="Event"/> objects.</returns>
        Task<IEnumerable<Event>> GetRecommendationsAsync(string? search, string? category, DateTime? date);

        /// <summary>
        /// Gets the set of unique event categories.
        /// </summary>
        /// <returns>A set of unique category names.</returns>
        Task<HashSet<string>> GetCategoriesAsync();
    }
}