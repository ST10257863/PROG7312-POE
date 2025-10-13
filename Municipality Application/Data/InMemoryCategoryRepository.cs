using Municipality_Application.Interfaces;
using Municipality_Application.Models;
using System.Collections.Concurrent;

namespace Municipality_Application.Data
{
    public class InMemoryCategoryRepository : ICategoryRepository
    {
        private readonly ConcurrentDictionary<int, Category> _categories = new();
        private int _nextId = 1;

        public Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return Task.FromResult(_categories.Values.AsEnumerable());
        }

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