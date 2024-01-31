using Crisp.Core.Models;
using Crisp.Core.Repositories;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

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

    public async Task<IEnumerable<SecurityBenchmark>?> GetBenchmarksAsync(string resourceName)
    {
        var repositoryDirectoryPath = await gitHubRepository.CloneAsync(GitHubAccountName, GitHubRepositoryName);

        var benchmarks = await securityBenchmarksRepository.GetSecurityBenchmarksForResourceAsync(resourceName, repositoryDirectoryPath);

        return benchmarks;
    }

    public async Task<IEnumerable<string>> GetResourcesAsync()
    {
        var repositoryDirectoryPath = await gitHubRepository.CloneAsync(GitHubAccountName, GitHubRepositoryName);
        return await securityBenchmarksRepository.GetAllResourceNamesAsync(repositoryDirectoryPath);
    }


    private Category MapBenchmarksToCategory(string resourceName, IEnumerable<SecurityBenchmark> benchmarks)
    {
        var categories = new List<Category>();
        if (benchmarks.All(b => string.IsNullOrEmpty(b.ControlTitle)))
        {
            // old benchmarks - v1.1, v2
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
        } else
        {
            // new benchmarks - v3
            var categoriesOrder = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                {"Network Security", 1},
                {"Identity Management", 2},
                {"Privileged Access", 3},
                {"Data Protection", 4},
                {"Asset Management", 5},
                {"Logging and Threat Detection", 6},
                {"Incident Response", 7},
                {"Posture and Vulnerability Management", 8},
                {"Endpoint Security", 9},
                {"Backup and Recovery", 10},
                {"DevOps Security", 11},
                {"Governance and Strategy", 12}
            };

            var sortedBenchmarksGroups = benchmarks.Where(b => categoriesOrder.ContainsKey(b.Category)).GroupBy(b => b.Category).OrderBy(g => categoriesOrder[g.Key]).ToList();

            foreach (var group in sortedBenchmarksGroups)
            {
                var subCategories = new List<Category>();
                foreach (var subGroup in group.GroupBy(b => b.Id).OrderBy(g => g.Key))
                {
                    var recommendations = subGroup.Select(b => MapSecurityBenchmarkToRecommendation($"{resourceName}-{group.Key}-{subGroup.Key}", b)).ToArray();
                    subCategories.Add(new Category(
                        GenerateIdFor($"{resourceName}-{group.Key}-{subGroup.Key}"),
                        subGroup.First().ControlTitle ?? "",
                        Description: null,
                        Children: Enumerable.Empty<Category>(),
                        recommendations));
                }
                categories.Add(new Category(
                    GenerateIdFor($"{resourceName}-{group.Key}"),
                    group.Key,
                    Description: null,
                    Children: subCategories,
                    Recommendations: Enumerable.Empty<Recommendation>()));
            }
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
        if (benchmark.ControlTitle is null)
        {
            // old benchmarks - v1.1, v2
            var description = benchmark.Description;
            var matches = Regex.Matches(description, "[\\s]+https://[^\\s]+");
            foreach (Match match in matches)
            {
                var url = match.Value.Trim();
                description = description.Replace(match.Value, $" [{url}]({url})");
            }
            return new Recommendation(
                GenerateIdFor($"{resourceName}-{benchmark.Title}"),
                benchmark.Title,
                description,
                null
            );
        } else
        {
            // new benchmarks - v3
            var featureDefined = !string.Equals(benchmark.FeatureDescription, "No Related Feature", StringComparison.OrdinalIgnoreCase);
            var description = "";
            if (featureDefined)
            {
                description = $"{benchmark.FeatureDescription}{Environment.NewLine}{Environment.NewLine}**Configuration Guidance:**{Environment.NewLine}{Environment.NewLine}{benchmark.Description}";
            } else
            {
                description = $"{benchmark.Description}";
            }
            if (!string.IsNullOrWhiteSpace(benchmark.FeatureReference) && featureDefined && !string.Equals(benchmark.FeatureReference, "None", StringComparison.OrdinalIgnoreCase))
            {
                var reference = benchmark.FeatureReference;
                var match = Regex.Match(benchmark.FeatureReference, "https://[^\\s]+");
                if (match.Success)
                {
                    reference = reference.Replace(match.Value, $"[{match.Value}]({match.Value})");
                }
                description += $"{Environment.NewLine}{Environment.NewLine}**Reference:**{Environment.NewLine}{Environment.NewLine}{reference}";
            }
            return new Recommendation(
                GenerateIdFor($"{resourceName}-{benchmark.FeatureName}"),
                benchmark.FeatureName!,
                description,
                null
            );
        }
    }

    private static string GenerateIdFor(string text)
    {
        return string.Join("", (SHA1.HashData(Encoding.UTF8.GetBytes(text))).Select(b => b.ToString("x2")));
    }
}
