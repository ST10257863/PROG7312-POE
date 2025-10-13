using Municipality_Application.Models;

namespace Municipality_Application.Interfaces
{
    /// <summary>
    /// Provides methods for managing and retrieving event data.
    /// </summary>
    public interface IEventRepository
    {
        /// <summary>
        /// Adds a new event to the data store.
        /// </summary>
        /// <param name="ev">The event to add.</param>
        /// <returns>The added <see cref="Event"/> object.</returns>
        Task<Event> AddEventAsync(Event ev);

        /// <summary>
        /// Retrieves an event by its unique identifier.
        /// </summary>
        /// <param name="id">The event's unique identifier.</param>
        /// <returns>The <see cref="Event"/> object if found; otherwise, null.</returns>
        Task<Event?> GetEventByIdAsync(int id);

        /// <summary>
        /// Retrieves all events.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="Event"/> objects.</returns>
        Task<IEnumerable<Event>> GetAllEventsAsync();

        /// <summary>
        /// Updates an existing event.
        /// </summary>
        /// <param name="ev">The event with updated data.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        Task<bool> UpdateEventAsync(Event ev);

        /// <summary>
        /// Deletes an event by its unique identifier.
        /// </summary>
        /// <param name="id">The event's unique identifier.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        Task<bool> DeleteEventAsync(int id);

        /// <summary>
        /// Increments the search frequency for a given search term.
        /// </summary>
        /// <param name="searchTerm">The search term to increment.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task IncrementSearchFrequencyAsync(string searchTerm);

        /// <summary>
        /// Retrieves the search frequency dictionary for all search terms.
        /// </summary>
        /// <returns>A dictionary mapping search terms to their frequency.</returns>
        Task<Dictionary<string, int>> GetSearchFrequencyAsync();

        /// <summary>
        /// Seeds the data store with default events.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SeedDefaultEventsAsync();
    }
}