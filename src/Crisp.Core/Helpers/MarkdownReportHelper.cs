using Crisp.Core.Models;
using System.Text;

namespace Crisp.Core.Helpers;

public static class MarkdownReportHelper
{
    public static string GenerateThreatModelPropertiesSection(ThreatModel threatModel,
        IDictionary<string, IEnumerable<SecurityBenchmark>>? benchmarks)
    {
        var section = new StringBuilder();
        var index = 1;
        foreach (var threat in threatModel.Threats)
        {
            if (index > 1)
            {
                section.AppendLine();
            }
            section.AppendLine("---");
            section.AppendLine($"**Threat #:** {index}  ");
            section.AppendLine(threat.Description.Trim());
            if (threatModel.AddResourcesRecommendations)
            {
                var resourcesRecommendations = GenerateResourcesRecommendationsForThreat(threat, benchmarks);
                if (!string.IsNullOrEmpty(resourcesRecommendations))
                {
                    section.AppendLine(resourcesRecommendations);
                }
            }
            index++;
        }
        return section.ToString().TrimEnd(Environment.NewLine.ToCharArray());
    }

    public static string GenerateResourcesRecommendationsForThreat(Threat threat,
        IDictionary<string, IEnumerable<SecurityBenchmark>>? benchmarks)
    {
        if (threat.BenchmarkIds is null || !threat.BenchmarkIds.Any() || benchmarks is null)
        {
            return "";
        }

        var section = new StringBuilder();
        section.AppendLine();
        section.AppendLine($"**Recommendations for resources:**");
        section.AppendLine();
        foreach (var resourceName in benchmarks.Keys)
        {
            var resourceBenchmarks = benchmarks[resourceName];
            if (resourceBenchmarks is null)
            {
                continue;
            }
            section.AppendLine($"**{resourceName}:**");
            section.AppendLine();
            var index = 1;
            foreach (var benchmarkId in threat.BenchmarkIds)
            {
                var benchmark = resourceBenchmarks.FirstOrDefault(b => b.Id == benchmarkId);
                if (benchmark is null)
                {
                    continue;
                }
                section.AppendLine($"**Recommendation #:** {index}");
                section.AppendLine();
                section.AppendLine(benchmark.Description);
                section.AppendLine();
                index++;
            }
        }
        section.AppendLine();

        return section.ToString();
    }

    public static string GenerateDataflowAttributeSection(ThreatModel threatModel)
    {
        var section = new StringBuilder();
        foreach (var a in threatModel.DataflowAttributes)
        {
            section.AppendLine($"| {a.Number.Trim()} | {a.Transport.Trim()} | {a.DataClassification.Trim()} | {a.Authentication.Trim()} | {a.Authorization.Trim()} | {a.Notes.Trim()} |");
        }
        return section.ToString().TrimEnd(Environment.NewLine.ToCharArray());
    }
}
