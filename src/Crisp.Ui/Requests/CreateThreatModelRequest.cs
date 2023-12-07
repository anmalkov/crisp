using Crisp.Ui.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace Crisp.Ui.Requests;

public record CreateThreatModelDto(
    string ProjectName,
    string? Description,
    IEnumerable<DataflowAttributeDto> DataflowAttributes,
    IEnumerable<RecommendationDto> Threats,
    IEnumerable<KeyValuePair<string, string>>? Images
);

public record struct CreateThreatModelRequest(
    [FromBody]
    CreateThreatModelDto Body
) : IHttpRequest;
