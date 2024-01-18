namespace Crisp.Core.Models;

public record SecurityBenchmark(
    string Id,
    string Category,
    string Title,
    string Description,
    string? ControlTitle,
    string? FeatureName,
    string? FeatureDescription,
    string? FeatureNotes,
    string? FeatureReference,
    string? Responsibility
);
