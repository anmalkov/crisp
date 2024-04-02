namespace Crisp.Core.Models;

public enum ThreatStatus
{
    NotEvaluated,
    NotMitigated,
    PartiallyMitigated,
    Mitigated
}

public enum ThreatRisk
{
    NotEvaluated,
    Critical,
    High,
    Medium,
    Low
}

public record Threat(
    string Id,
    string Title,
    string Description,
    ThreatStatus Status,
    ThreatRisk Risk,
    int OrderIndex,
    IEnumerable<ThreatRecommendation>? Recommendations,
    IEnumerable<string>? BenchmarkIds
);
