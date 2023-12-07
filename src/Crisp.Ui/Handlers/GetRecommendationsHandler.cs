using Crisp.Core.Services;
using Crisp.Ui.Requests;
using MediatR;

namespace Crisp.Ui.Handlers;

public class GetRecommendationsHandler : IRequestHandler<GetRecommendationsRequest, IResult>
{
    private readonly IRecommendationsService recommendationsService;

    public GetRecommendationsHandler(IRecommendationsService recommendationsService)
    {
        this.recommendationsService = recommendationsService;
    }

    public async Task<IResult> Handle(GetRecommendationsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var recommendations = await recommendationsService.GetRecommendationsAsync(request.Body.Resources);
            return Results.Ok(recommendations);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }
}
