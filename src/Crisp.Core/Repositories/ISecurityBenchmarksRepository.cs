using Crisp.Core.Models;

namespace Crisp.Core.Repositories;

public interface ISecurityBenchmarksRepository
{
    Task<IEnumerable<string>> GetAllResourceNamesAsync(string rootDirectoryPath);
    Task<IEnumerable<SecurityBenchmark>> GetSecurityBenchmarksForResourceAsync(string resourceName, string rootDirectoryPath);
    Task<IEnumerable<SecurityBenchmarkControl>> GetSecurityBenchmarkControlsAsync(string rootDirectoryPath);
}