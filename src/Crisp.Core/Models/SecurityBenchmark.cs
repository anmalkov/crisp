namespace Crisp.Core.Models;

public record SecurityBenchmark(
    string Category,
    string AzureId,
    string Title,
    string Description,
    string? Responsibility
);
