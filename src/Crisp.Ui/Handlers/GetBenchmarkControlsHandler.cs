using Crisp.Core.Services;
using Crisp.Ui.Requests;
using MediatR;

namespace Crisp.Ui.Handlers;

public class GetBenchmarkControlsHandler : IRequestHandler<GetBenchmarkControlsRequest, IResult>
{
    private readonly IRecommendationsService recommendationsService;

    public GetBenchmarkControlsHandler(IRecommendationsService recommendationsService)
    {
        this.recommendationsService = recommendationsService;
    }

    public async Task<IResult> Handle(GetBenchmarkControlsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var controlsCategory = await recommendationsService.GetBenchmarkControlsAsync();
            return Results.Ok(controlsCategory);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }
}
