namespace Crisp.Core.Models;

public record GitHubFile(
    string Name,
    string Url,
    string? Content,
    byte[]? BinaryContent
);
