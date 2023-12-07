using Crisp.Core.Models;

namespace Crisp.Core.Services;

public interface IThreatModelsService
{
    Task<IEnumerable<ThreatModel>?> GetAllAsync();
    Task<ThreatModel?> GetAsync(string id);
    Task<Category?> GetCategoryAsync();
    Task CreateAsync(ThreatModel threatModel);
    Task UpdateAsync(ThreatModel threatModel);
    Task DeleteAsync(string id);
    Task<string?> GetReportAsync(string threatModelId);
    Task<(byte[]? archiveContent, string fileName)> GetReportArchiveAsync(string threatModelId);
    Task StoreFileForReportAsync(string threatModelId, string fileName, byte[] content);
    Task<byte[]?> GetReportFileAsync(string threatModelId, string fileName);
}
