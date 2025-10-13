using Municipality_Application.Models;

namespace Municipality_Application.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task SeedDefaultCategoriesAsync();
    }
}