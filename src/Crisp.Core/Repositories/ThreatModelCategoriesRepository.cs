using Crisp.Core.Models;
using System.Reflection;
using System.Text.Json;

namespace Crisp.Core.Repositories;

public class ThreatModelCategoriesRepository : RepositoryBase<Category>, IThreatModelCategoriesRepository
{
    private const string RepositoryFilename = "threatmodels-categories.json";

    public ThreatModelCategoriesRepository() : base(RepositoryFilename) { }


    public new async Task<Category?> GetAllAsync()
    {
        var categories = await base.GetAllAsync();
        return categories is not null && categories.Count() > 0 ? categories.First() : null;
    }

    public async Task UpdateAllAsync(Category category)
    {
        await base.UpdateAllAsync(new[] { category });
    }
}
