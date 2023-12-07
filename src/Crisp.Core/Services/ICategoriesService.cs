using Crisp.Core.Models;

namespace Crisp.Core.Services;

public interface ICategoriesService
{
    Task<Category?> GetAsync();
}