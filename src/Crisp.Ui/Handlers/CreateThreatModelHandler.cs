using Crisp.Core.Models;
using Crisp.Core.Services;
using Crisp.Ui.Requests;
using MediatR;

namespace Crisp.Ui.Handlers
{
    public class CreateThreatModelHandler : IRequestHandler<CreateThreatModelRequest, IResult>
    {
        private readonly IThreatModelsService _threatModelsService;

        public CreateThreatModelHandler(IThreatModelsService threatModelsService)
        {
            _threatModelsService = threatModelsService;
        }

        public async Task<IResult> Handle(CreateThreatModelRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var threatModel = MapRequestToThreatModel(request.Body);
                await _threatModelsService.CreateAsync(threatModel);
                return Results.Ok(new { threatModel.Id });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }

        private static ThreatModel MapRequestToThreatModel(CreateThreatModelDto dto)
        {
            return new ThreatModel(
                Guid.NewGuid().ToString(),
                dto.ProjectName,
                dto.Description,
                DateTime.Now,
                null,
                dto.AddResourcesRecommendations,
                dto.DataflowAttributes.Select(MapDtoToDataflowAttribute).ToArray(),
                dto.Threats.Select(MapDtoToThreat).ToArray(),
                dto.Images?.ToDictionary(i => i.Key, i => i.Value),
                dto.Resources
            );
        }

        private static DataflowAttribute MapDtoToDataflowAttribute(DataflowAttributeDto dto)
        {
            return new DataflowAttribute(
                dto.Number,
                dto.Transport,
                dto.DataClassification,
                dto.Authentication,
                dto.Authorization,
                dto.Notes
            );
        }

        private static Threat MapDtoToThreat(ThreatDto dto)
        {
            return new Threat(
                dto.Id,
                dto.Title,
                dto.Description,
                dto.Status,
                dto.Risk,
                dto.OrderIndex,
                dto.Recommendations?.Select(MapDtoToThreatRecommendation).ToArray(),
                dto.BenchmarkIds
            );
        }

        private static ThreatRecommendation MapDtoToThreatRecommendation(ThreatRecommendationDto dto)
        {
            return new ThreatRecommendation(
                dto.Id,
                dto.Title,
                dto.Description,
                dto.OrderIndex
            );
        }
    }
}
