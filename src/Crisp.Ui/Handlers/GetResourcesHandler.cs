using Crisp.Core.Services;
using Crisp.Ui.Requests;
using MediatR;

namespace Crisp.Ui.Handlers;

public record ResourcesDto(IEnumerable<string> Resources);

public class GetResourcesHandler : IRequestHandler<GetResourcesRequest, IResult>
{
    private readonly IRecommendationsService recommendationsService;

    public GetResourcesHandler(IRecommendationsService recommendationsService)
    {
        this.recommendationsService = recommendationsService;
    }

    public async Task<IResult> Handle(GetResourcesRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var resources = await recommendationsService.GetResourcesAsync();
            return Results.Ok(new ResourcesDto(resources));
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }
}
