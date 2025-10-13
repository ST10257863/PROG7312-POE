using Municipality_Application.Interfaces;
using Municipality_Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Municipality_Application.Data
{
    /// <summary>
    /// Entity Framework Core implementation of <see cref="ICategoryRepository"/> for managing category data in the database.
    /// </summary>
    public class EfCategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="EfCategoryRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The application's database context.</param>
        public EfCategoryRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves all categories from the database.
        /// </summary>
        /// <returns>A list of all <see cref="Category"/> entities.</returns>
        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _dbContext.Categories.ToListAsync();
        }

        /// <summary>
        /// Seeds the database with default categories if none exist.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
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