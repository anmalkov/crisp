using Crisp.Core.Models;
using System.Diagnostics;
using System.Reflection;

namespace Crisp.Core.Repositories;

public class GitHubGitRepository : IGitHubRepository
{
    private const string GitHubRepositoriesDirectoryName = "repos";

    private readonly string _repositoriesFullPath;

    private record GitHubDto(string Name, string Url, string Type, string? Content);

    public GitHubGitRepository()
    {
        _repositoriesFullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", GitHubRepositoriesDirectoryName);
    }

    public async Task<GitHubDirectory> GetContentAsync(string accountName, string repositoryName, string folderName)
    {
        if (string.IsNullOrWhiteSpace(accountName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(accountName));
        }
        if (string.IsNullOrWhiteSpace(repositoryName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(repositoryName));
        }
        if (string.IsNullOrWhiteSpace(folderName))
        {
            folderName = ".";
        }

        await CloneAsync(accountName, repositoryName);

        var repositoryDirectoryPath = GetRepositoryDirectoryPath(repositoryName);
        var directoryPath = Path.Combine(repositoryDirectoryPath, folderName);
        var directoryName = folderName.Split(Path.DirectorySeparatorChar).Last();
        return GetDirectory(directoryName, directoryPath);
    }

    public async Task<GitHubFile> GetFileAsync(string accountName, string repositoryName, string path)
    {
        await CloneAsync(accountName, repositoryName);

        var repositoryDirectoryPath = GetRepositoryDirectoryPath(repositoryName);
        var filePath = Path.Combine(repositoryDirectoryPath, path);
        return await GetFileAsync(filePath);
    }

    public async Task<string> CloneAsync(string accountName, string repositoryName)
    {
        var repositoryDirectoryPath = GetRepositoryDirectoryPath(repositoryName);
        if (RepositoryCloned(repositoryName))
        {
            return repositoryDirectoryPath;
        }

        if (!Directory.Exists(repositoryDirectoryPath))
        {
            Directory.CreateDirectory(repositoryDirectoryPath);
        }
        
        var repositoryUrl = $"https://github.com/{accountName}/{repositoryName}.git";
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"clone {repositoryUrl} {repositoryDirectoryPath}",
            }
        };
        process.Start();
        await process.WaitForExitAsync();

        return repositoryDirectoryPath;
    }


    private bool RepositoryCloned(string repositoryName)
    {
        var repositoryDirectoryPath = GetRepositoryDirectoryPath(repositoryName);
        if (!Directory.Exists(repositoryDirectoryPath))
        {
            return false;
        }

        var directories = Directory.GetDirectories(repositoryDirectoryPath);
        if (directories.Any())
        {
            return true;
        }

        var files = Directory.GetFiles(repositoryDirectoryPath);
        return files.Any();
    }

    private string GetRepositoryDirectoryPath(string repositoryName)
    {
        return Path.Combine(_repositoriesFullPath, repositoryName);
    }

    private GitHubDirectory GetDirectory(string name, string path)
    {
        var files = Directory.GetFiles(path).Select(async p => await GetFileAsync(p)).Select(t => t.Result).ToArray();
        var directories = Directory.GetDirectories(path).Select(p => GetDirectory(p.Split(Path.DirectorySeparatorChar).Last(), p)).ToArray();
        return new GitHubDirectory(
            name,
            path,
            directories,
            files
        );
    }

    private static async Task<GitHubFile> GetFileAsync(string path)
    {
        var isMarkdownFile = Path.GetExtension(path).ToLower() == ".md";
        return new GitHubFile(
            Path.GetFileName(path),
            path,
            isMarkdownFile ? await File.ReadAllTextAsync(path) : null,
            isMarkdownFile ? null : await File.ReadAllBytesAsync(path)
        );
    }
}
