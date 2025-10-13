using Municipality_Application.Models;

namespace Municipality_Application.Interfaces
{
    public interface IEventRepository
    {
        Task<Event> AddEventAsync(Event ev);
        Task<Event?> GetEventByIdAsync(int id);
        Task<IEnumerable<Event>> GetAllEventsAsync();
        Task<bool> UpdateEventAsync(Event ev);
        Task<bool> DeleteEventAsync(int id);
    }
}