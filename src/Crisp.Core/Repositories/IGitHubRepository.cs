using Crisp.Core.Models;

namespace Crisp.Core.Repositories;

public interface IGitHubRepository
{
    Task<string> CloneAsync(string accountName, string repositoryName);
    Task<GitHubDirectory> GetContentAsync(string accountName, string repositoryName, string folderName);
    Task<GitHubFile> GetFileAsync(string accountName, string repositoryName, string path);
}