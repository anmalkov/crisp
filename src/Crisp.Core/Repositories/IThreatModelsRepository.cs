using Crisp.Core.Models;

namespace Crisp.Core.Repositories;

public interface IThreatModelsRepository
{
    Task<IEnumerable<ThreatModel>?> GetAllAsync();
    Task<ThreatModel?> GetAsync(string id);
    Task CreateAsync(ThreatModel item);
    Task UpdateAsync(ThreatModel item);
    Task DeleteAsync(string id);
}