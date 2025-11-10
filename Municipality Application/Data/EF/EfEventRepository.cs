using Municipality_Application.Interfaces;
using Municipality_Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Municipality_Application.Data.EF
{
    /// <summary>
    /// Entity Framework Core implementation of <see cref="IEventRepository"/> for managing event data in the database.
    /// </summary>
    public class EfEventRepository : IEventRepository
    {
        private readonly AppDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="EfEventRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The application's database context.</param>
        public EfEventRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Adds a new event to the database, including its address.
        /// </summary>
        /// <param name="ev">The event to add.</param>
        /// <returns>The added <see cref="Event"/> entity.</returns>
        public async Task<Event> AddEventAsync(Event ev)
        {
            // Ensure Address is added and tracked
            if (ev.Address != null)
            {
                if (ev.Address.Id == 0)
                {
                    _dbContext.Addresses.Add(ev.Address);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    if (!_dbContext.Addresses.Local.Any(a => a.Id == ev.Address.Id))
                        _dbContext.Addresses.Attach(ev.Address);
                }
                ev.AddressId = ev.Address.Id;
            }

            _dbContext.Events.Add(ev);
            await _dbContext.SaveChangesAsync();
            return ev;
        }

        /// <summary>
        /// Retrieves an event by its unique identifier, including its address.
        /// </summary>
        /// <param name="id">The event's unique identifier.</param>
        /// <returns>The <see cref="Event"/> entity if found; otherwise, null.</returns>
        public async Task<Event?> GetEventByIdAsync(int id)
        {
            return await _dbContext.Events
                .Include(e => e.Address)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        /// <summary>
        /// Retrieves all events from the database, including their addresses.
        /// </summary>
        /// <returns>A list of all <see cref="Event"/> entities.</returns>
        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            return await _dbContext.Events
                .Include(e => e.Address)
                .ToListAsync();
        }

        /// <summary>
        /// Updates an existing event in the database, including its address.
        /// </summary>
        /// <param name="ev">The event with updated data.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public async Task<bool> UpdateEventAsync(Event ev)
        {
            var existing = await _dbContext.Events
                .Include(e => e.Address)
                .FirstOrDefaultAsync(e => e.Id == ev.Id);
            if (existing == null)
                return false;

            // Update Address
            if (ev.Address != null)
            {
                if (existing.Address == null || existing.Address.Id != ev.Address.Id)
                {
                    _dbContext.Addresses.Add(ev.Address);
                    await _dbContext.SaveChangesAsync();
                    ev.AddressId = ev.Address.Id;
                }
                else
                {
                    _dbContext.Entry(existing.Address).CurrentValues.SetValues(ev.Address);
                }
            }

            _dbContext.Entry(existing).CurrentValues.SetValues(ev);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Deletes an event from the database, optionally removing its address if not referenced elsewhere.
        /// </summary>
        /// <param name="id">The event's unique identifier.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        public async Task<bool> DeleteEventAsync(int id)
        {
            var ev = await _dbContext.Events
                .Include(e => e.Address)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (ev == null)
                return false;

            _dbContext.Events.Remove(ev);

            // Optionally remove the address if not referenced by any other event
            if (ev.AddressId != 0)
            {
                bool addressInUse = await _dbContext.Events.AnyAsync(e => e.AddressId == ev.AddressId && e.Id != id);
                if (!addressInUse && ev.Address != null)
                {
                    _dbContext.Addresses.Remove(ev.Address);
                }
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Increments the search frequency for a given search term.
        /// </summary>
        /// <param name="searchTerm">The search term to increment.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task IncrementSearchFrequencyAsync(string searchTerm)
        {
            var key = searchTerm.ToLower();
            var entry = await _dbContext.EventSearchFrequencies.FindAsync(key);
            if (entry == null)
            {
                entry = new EventSearchFrequency { SearchTerm = key, Frequency = 1 };
                _dbContext.EventSearchFrequencies.Add(entry);
            }
            else
            {
                entry.Frequency += 1;
                _dbContext.EventSearchFrequencies.Update(entry);
            }
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves the search frequency dictionary for all search terms.
        /// </summary>
        /// <returns>A dictionary mapping search terms to their frequency.</returns>
        public async Task<Dictionary<string, int>> GetSearchFrequencyAsync()
        {
            return await _dbContext.EventSearchFrequencies
                .ToDictionaryAsync(e => e.SearchTerm, e => e.Frequency);
        }
    }
}