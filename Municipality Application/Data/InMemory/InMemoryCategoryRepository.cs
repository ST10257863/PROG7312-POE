using Municipality_Application.Interfaces;
using Municipality_Application.Models;
using System.Collections.Concurrent;

namespace Municipality_Application.Data.InMemory
{
    /// <summary>
    /// In-memory implementation of <see cref="ICategoryRepository"/> for managing category data during application runtime.
    /// </summary>
    public class InMemoryCategoryRepository : ICategoryRepository
    {
        private readonly ConcurrentDictionary<int, Category> _categories = new();
        private int _nextId = 1;

        /// <summary>
        /// Retrieves all categories from the in-memory store.
        /// </summary>
        /// <returns>A list of all <see cref="Category"/> entities.</returns>
        public Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return Task.FromResult(_categories.Values.AsEnumerable());
        }

        /// <summary>
        /// Sets all categories in the in-memory store, typically used by the data seeder.
        /// </summary>
        /// <param name="categories">The categories to set.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task SetAllCategoriesAsync(IEnumerable<Category> categories)
        {
            _categories.Clear();
            _nextId = 1;
            foreach (var cat in categories)
            {
                if (cat.Id == 0)
                    cat.Id = _nextId++;
                else
                    _nextId = Math.Max(_nextId, cat.Id + 1);
                _categories[cat.Id] = cat;
            }
            return Task.CompletedTask;
        }
    }
}