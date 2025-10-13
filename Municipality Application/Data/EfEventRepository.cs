using Municipality_Application.Interfaces;
using Municipality_Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Municipality_Application.Data
{
    public class EfEventRepository : IEventRepository
    {
        private readonly AppDbContext _dbContext;

        public EfEventRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Event> AddEventAsync(Event ev)
        {
            _dbContext.Events.Add(ev);
            await _dbContext.SaveChangesAsync();
            return ev;
        }

        public async Task<Event?> GetEventByIdAsync(int id)
        {
            return await _dbContext.Events.FindAsync(id);
        }

        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            return await _dbContext.Events.ToListAsync();
        }

        public async Task<bool> UpdateEventAsync(Event ev)
        {
            var existing = await _dbContext.Events.FindAsync(ev.Id);
            if (existing == null)
                return false;

            _dbContext.Entry(existing).CurrentValues.SetValues(ev);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteEventAsync(int id)
        {
            var ev = await _dbContext.Events.FindAsync(id);
            if (ev == null)
                return false;

            _dbContext.Events.Remove(ev);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}