using Crisp.Core.Models;
using Crisp.Core.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace Crisp.Core.Services;

public class RecommendationsService : IRecommendationsService
{
    private const string GitHubAccountName = "MicrosoftDocs";
    private const string GitHubRepositoryName = "SecurityBenchmarks";

    private readonly IGitHubRepository gitHubRepository;
    private readonly ISecurityBenchmarksRepository securityBenchmarksRepository;

    private IDictionary<string, string> resourceNames = new Dictionary<string, string>
    {
        { "App Service", "Azure App Service, AppService, Azure Web App, Web App" }
    };

    public RecommendationsService(IGitHubRepository gitHubRepository, ISecurityBenchmarksRepository securityBenchmarksRepository)
    {
        this.gitHubRepository = gitHubRepository;
        this.securityBenchmarksRepository = securityBenchmarksRepository;
    }

    public async Task<Category> GetRecommendationsAsync(IEnumerable<string> resources)
    {
        var repositoryDirectoryPath = await gitHubRepository.CloneAsync(GitHubAccountName, GitHubRepositoryName);

        var categories = new List<Category>();
        foreach (var resourceName in resources)
        {
            var benchmarks = await securityBenchmarksRepository.GetSecurityBenchmarksForResourceAsync(resourceName, repositoryDirectoryPath);
            categories.Add(MapBenchmarksToCategory(resourceName, benchmarks));
        }

        return new Category(
            GenerateIdFor("Resources"),
            "Resources",
            Description: null,
            categories,
            Recommendations: Enumerable.Empty<Recommendation>());
    }

    public async Task<IEnumerable<string>> GetResourcesAsync()
    {
        var repositoryDirectoryPath = await gitHubRepository.CloneAsync(GitHubAccountName, GitHubRepositoryName);
        return await securityBenchmarksRepository.GetAllResourceNamesAsync(repositoryDirectoryPath);
    }


    private Category MapBenchmarksToCategory(string resourceName, IEnumerable<SecurityBenchmark> benchmarks)
    {
        var categories = new List<Category>();
        foreach (var categoryName in benchmarks.Select(b => b.Category).Distinct())
        {
            var recommendations = benchmarks.Where(b => b.Category.Equals(categoryName)).Select(b => MapSecurityBenchmarkToRecommendation(resourceName, b)).ToArray();
            categories.Add(new Category(
                GenerateIdFor($"{resourceName}-{categoryName}"),
                categoryName,
                Description: null,
                Children: Enumerable.Empty<Category>(),
                recommendations));
        }
        return new Category(
            GenerateIdFor(resourceName),
            resourceName,
            Description: null,
            categories,
            Recommendations: Enumerable.Empty<Recommendation>());
    }

    private Recommendation MapSecurityBenchmarkToRecommendation(string resourceName, SecurityBenchmark benchmark)
    {
        return new Recommendation(
            GenerateIdFor($"{resourceName}-{benchmark.Title}"),
            benchmark.Title,
            benchmark.Description
        );
    }

    private static string GenerateIdFor(string text)
    {
        return string.Join("", (SHA1.HashData(Encoding.UTF8.GetBytes(text))).Select(b => b.ToString("x2")));
    }
}
