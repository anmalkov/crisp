using Crisp.Core.Models;
using Crisp.Ui.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace Crisp.Ui.Requests;

public record ThreatRecommendationDto(
    string Id,
    string Title,
    string Description,
    int OrderIndex
);

public record ThreatDto(
    string Id,
    string Title,
    string Description,
    ThreatStatus Status,
    ThreatRisk Risk,
    int OrderIndex,
    IEnumerable<ThreatRecommendationDto>? Recommendations,
    IEnumerable<string>? BenchmarkIds
);

public record CreateThreatModelDto(
    string ProjectName,
    string? Description,
    bool AddResourcesRecommendations,
    IEnumerable<DataflowAttributeDto> DataflowAttributes,
    IEnumerable<ThreatDto> Threats,
    IEnumerable<KeyValuePair<string, string>>? Images,
    IEnumerable<string>? Resources
);

public record struct CreateThreatModelRequest(
    [FromBody]
    CreateThreatModelDto Body
) : IHttpRequest;
