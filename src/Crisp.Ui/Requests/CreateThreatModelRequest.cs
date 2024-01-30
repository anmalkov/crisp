using Crisp.Ui.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace Crisp.Ui.Requests;

public record CreateThreatModelDto(
    string ProjectName,
    string? Description,
    bool AddResourcesRecommendations,
    IEnumerable<DataflowAttributeDto> DataflowAttributes,
    IEnumerable<RecommendationDto> Threats,
    IEnumerable<KeyValuePair<string, string>>? Images,
    IEnumerable<string>? Resources
);

public record struct CreateThreatModelRequest(
    [FromBody]
    CreateThreatModelDto Body
) : IHttpRequest;
