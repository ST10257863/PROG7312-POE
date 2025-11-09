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
        /// Seeds the in-memory store with default categories if none exist.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task SeedDefaultCategoriesAsync()
        {
            if (_categories.IsEmpty)
            {
                var defaultNames = new[] { "Roads", "Water", "Electricity", "Sanitation", "Other" };
                foreach (var name in defaultNames)
                {
                    var category = new Category { Id = _nextId++, Name = name };
                    _categories[category.Id] = category;
                }
            }
            return Task.CompletedTask;
        }
    }
}