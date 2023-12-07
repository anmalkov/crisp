namespace Crisp.Core.Models;

public record Category(
    string Id,
    string Name,
    string? Description,
    IEnumerable<Category>? Children,
    IEnumerable<Recommendation>? Recommendations
) : IStorableItem;
