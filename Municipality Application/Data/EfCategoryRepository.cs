using Municipality_Application.Interfaces;
using Municipality_Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Municipality_Application.Data
{
    public class EfCategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _dbContext;

        public EfCategoryRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _dbContext.Categories.ToListAsync();
        }

        public async Task SeedDefaultCategoriesAsync()
        {
            if (!await _dbContext.Categories.AnyAsync())
            {
                var defaultCategories = new List<Category>
                {
                    new Category { Name = "Roads" },
                    new Category { Name = "Water" },
                    new Category { Name = "Electricity" },
                    new Category { Name = "Sanitation" },
                    new Category { Name = "Other" }
                };

                _dbContext.Categories.AddRange(defaultCategories);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}