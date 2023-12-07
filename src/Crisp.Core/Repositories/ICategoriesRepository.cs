using Crisp.Core.Models;

namespace Crisp.Core.Repositories;

public interface ICategoriesRepository
{
    Task<Category?> GetAllAsync();
    Task UpdateAllAsync(Category category);
}