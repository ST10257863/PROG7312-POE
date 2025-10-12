using Municipality_Application.Models;

namespace Municipality_Application.Interfaces
{
    public interface IEventService
    {
        /// <summary>
        /// Retrieves events filtered by optional search keyword, category, or date.
        /// </summary>
        IEnumerable<Event> GetEvents(string? search, string? category, DateTime? date);

        /// <summary>
        /// Returns recommended events based on user search behavior or category similarity.
        /// </summary>
        IEnumerable<Event> GetRecommendations(string? search, string? category, DateTime? date);

        /// <summary>
        /// Gets the set of unique event categories.
        /// </summary>
        HashSet<string> GetCategories();
    }
}