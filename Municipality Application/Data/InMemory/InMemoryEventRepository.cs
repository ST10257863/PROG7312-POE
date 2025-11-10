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
        /// Sets all events in the in-memory store (for seeding purposes).
        /// </summary>
        /// <param name="events">The events to set.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task SetAllEventsAsync(IEnumerable<Event> events)
        {
            _events.Clear();
            _addresses.Clear();
            _nextId = 1;
            _nextAddressId = 1;

            foreach (var ev in events)
            {
                if (ev.Id == 0)
                    ev.Id = _nextId++;
                else
                    _nextId = Math.Max(_nextId, ev.Id + 1);

                if (ev.Address != null)
                {
                    if (ev.Address.Id == 0)
                        ev.Address.Id = _nextAddressId++;
                    else
                        _nextAddressId = Math.Max(_nextAddressId, ev.Address.Id + 1);

                    _addresses[ev.Address.Id] = ev.Address;
                    ev.AddressId = ev.Address.Id;
                }
                _events[ev.Id] = ev;
            }
            return Task.CompletedTask;
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