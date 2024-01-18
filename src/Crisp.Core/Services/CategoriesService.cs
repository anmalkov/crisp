using Crisp.Core.Models;
using Crisp.Core.Repositories;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Crisp.Core.Services;

public class CategoriesService : ICategoriesService
{
    private const string GitHubAccountName = "anmalkov";
    private const string GitHubRepositoryName = "brief";
    private const string GitHubRecommendationsFolderName = "Security Domain";

    private const string CategoryCacheKey = "category";

    private readonly IGitHubRepository _gitHubRepository;
    private readonly ICategoriesRepository _categoriesRepository;
    private readonly IMemoryCache _memoryCache;

    public CategoriesService(IGitHubRepository gitHubRepository, ICategoriesRepository categoriesRepository, IMemoryCache memoryCache)
    {
        _gitHubRepository = gitHubRepository;
        _categoriesRepository = categoriesRepository;
        _memoryCache = memoryCache;
    }

    public async Task<Category?> GetAsync()
    {
        return await _memoryCache.GetOrCreateAsync(CategoryCacheKey, async entry =>
        {
            entry.SetPriority(CacheItemPriority.NeverRemove);
            var category = await _categoriesRepository.GetAllAsync();
            if (category is null)
            {
                category = await GetRecommendationsFromGitHubAsync();
                await _categoriesRepository.UpdateAllAsync(category);
            }
            return category;
        });
    }
    
    private async Task<Category> GetRecommendationsFromGitHubAsync()
    {
        var directory = await _gitHubRepository.GetContentAsync(GitHubAccountName, GitHubRepositoryName, GitHubRecommendationsFolderName);

        return MapDirectoryToCategory(directory);
    }

    private Category MapDirectoryToCategory(GitHubDirectory directory)
    {
        var descriptionFile = directory.Files?.Where(f => f.Name == ".description.md").FirstOrDefault();
        var description = descriptionFile?.Content ?? "";
        return new Category(
            GenerateIdFor(directory.Url),
            directory.Name,
            description,
            directory.Directories?.Select(d => MapDirectoryToCategory(d)).ToList(),
            directory.Files?.Where(f => f.Name != ".description.md").Select(f => MapFileToRecommendation(f)).ToList()
        );
    }

    private static string GenerateIdFor(string text)
    {
        return string.Join("", (SHA1.HashData(Encoding.UTF8.GetBytes(text))).Select(b => b.ToString("x2")));
    }

    private Recommendation MapFileToRecommendation(GitHubFile file)
    {
        return new Recommendation(
            GenerateIdFor(file.Url),
            Path.GetFileNameWithoutExtension(file.Name),
            file.Content,
            null
        );
    }
}
