using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Crisp.Core.Models;

public record DataflowAttribute(
    string Number,
    string Transport,
    string DataClassification,
    string Authentication,
    string Authorization,
    string Notes
);

public record ThreatModel(
    string Id,
    string ProjectName,
    string? Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    bool AddResourcesRecommendations,
    IEnumerable<DataflowAttribute> DataflowAttributes,
    IEnumerable<Threat> Threats,
    IDictionary<string, string>? Images,
    IEnumerable<string>? Resources
) : IStorableItem;