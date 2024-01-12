namespace Crisp.Core.Models;

public record SecurityBenchmark(
    string Category,
    string Title,
    string Description,
    string? AzureId,
    string? ControlId,
    string? ControlTitle,
    string? FeatureName,
    string? FeatureDescription,
    string? FeatureNotes,
    string? FeatureReference,
    string? Responsibility
);
