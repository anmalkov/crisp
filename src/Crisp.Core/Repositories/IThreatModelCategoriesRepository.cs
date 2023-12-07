using Crisp.Core.Models;

namespace Crisp.Core.Repositories;

public interface IThreatModelCategoriesRepository
{
    Task<Category?> GetAllAsync();
    Task UpdateAllAsync(Category category);
}