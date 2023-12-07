using Crisp.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Crisp.Core.Repositories;

public class GitHubApiRepository : IGitHubRepository
{
    private readonly HttpClient _httpClient;

    private record GitHubDto(string Name, string Url, string Type, string? Content);

    public GitHubApiRepository(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "brief");
        _httpClient.DefaultRequestHeaders.Add("Host", "api.github.com");
    }

    public Task<string> CloneAsync(string accountName, string repositoryName)
    {
        throw new NotImplementedException();
    }

    public Task<GitHubDirectory> GetContentAsync(string accountName, string repositoryName, string folderName)
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
        
        var url = $"https://api.github.com/repos/{accountName}/{repositoryName}/contents/{Uri.EscapeDataString(folderName)}";
        var directoryName = folderName.Split('/').Last();
        return GetDirectoryAsync(directoryName, url);
    }

    public async Task<GitHubFile> GetFileAsync(string accountName, string repositoryName, string path)
    {
        var url = $"https://api.github.com/repos/{accountName}/{repositoryName}/contents/{Uri.EscapeDataString(path)}";
        return await GetFileAsync(url);
    }
    

    private async Task<GitHubDirectory> GetDirectoryAsync(string name, string url)
    {
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Unknown response from GitHub. Status Code: ${response.StatusCode}");
        }

        var dtos = await response.Content.ReadFromJsonAsync<IEnumerable<GitHubDto>>();
        var files = dtos!.Where(f => f.Type == "file").Select(async f => await GetFileAsync(f.Url)).Select(t => t.Result).ToList();
        var directories = dtos!.Where(d => d.Type == "dir").Select(async d => await GetDirectoryAsync(d.Name, d.Url)).Select(t => t.Result).ToList();
        return new GitHubDirectory(
            name,
            url,
            directories,
            files
        );
    }

    private async Task<GitHubFile> GetFileAsync(string url)
    {
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Unknown response from GitHub. Status Code: ${response.StatusCode}");
        }

        var dto = await response.Content.ReadFromJsonAsync<GitHubDto>();
        var isMarkdownFile = url.ToLower().EndsWith(".md");
        var fileContent = Convert.FromBase64String(dto!.Content ?? "");
        return new GitHubFile(
            dto!.Name,
            url,
            isMarkdownFile ? Encoding.UTF8.GetString(fileContent) : null,
            isMarkdownFile ? null : fileContent
        );
    }

}
