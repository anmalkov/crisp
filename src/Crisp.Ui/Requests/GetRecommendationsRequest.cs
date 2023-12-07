using Microsoft.AspNetCore.Mvc;

namespace Crisp.Ui.Requests;

public record GetRecommendationsDto(
    IEnumerable<string> Resources
);

public record struct GetRecommendationsRequest(
    [FromBody]
    GetRecommendationsDto Body
) : IHttpRequest;
