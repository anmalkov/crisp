namespace Crisp.Core.Models;

public record SecurityBenchmarkControl(
    string Id,
    string Domain,
    string Title,
    string? Description,
    string? Azure,
    string? Aws,
    string? Gcp
);
