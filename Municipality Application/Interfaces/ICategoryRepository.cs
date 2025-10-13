using Municipality_Application.Models;

namespace Municipality_Application.Interfaces
{
    /// <summary>
    /// Provides methods for managing and retrieving category data.
    /// </summary>
    public interface ICategoryRepository
    {
        /// <summary>
        /// Retrieves all categories.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="Category"/> objects.</returns>
        Task<IEnumerable<Category>> GetAllCategoriesAsync();

        /// <summary>
        /// Seeds the data store with default categories if none exist.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SeedDefaultCategoriesAsync();
    }
}