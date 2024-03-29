﻿using Crisp.Core.Models;
using Crisp.Core.Services;
using Crisp.Ui.Requests;
using MediatR;
using System.Security.Cryptography.Xml;

namespace Crisp.Ui.Handlers
{
    public record DataflowAttributeDto(
        string Number,
        string Transport,
        string DataClassification,
        string Authentication,
        string Authorization,
        string Notes
    );
    
    public record ThreatModelDto(
        string Id,
        string ProjectName,
        string? Description,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        bool AddResourcesRecommendations,
        IEnumerable<DataflowAttributeDto> DataflowAttributes,
        IEnumerable<RecommendationDto> Threats,
        IEnumerable<KeyValuePair<string, string>>? Images,
        IEnumerable<string>? Resources
    );

    public class GetThreatModelsHandler : IRequestHandler<GetThreatModelsRequest, IResult>
    {
        private readonly IThreatModelsService _threatModelsService;


        public GetThreatModelsHandler(IThreatModelsService threatModelsService)
        {
            _threatModelsService = threatModelsService;
        }


        public async Task<IResult> Handle(GetThreatModelsRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var threatModels = await _threatModelsService.GetAllAsync();
                return Results.Ok(MapThreatModelsToDtos(threatModels));
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }
        

        private static IEnumerable<ThreatModelDto>? MapThreatModelsToDtos(IEnumerable<ThreatModel>? threatModels)
        {
            if (threatModels is null)
            {
                return new List<ThreatModelDto>();
            }
            
            return threatModels.Select(p => new ThreatModelDto(
                p.Id,
                p.ProjectName,
                p.Description,
                p.CreatedAt,
                p.UpdatedAt,
                p.AddResourcesRecommendations,
                p.DataflowAttributes.Select(MapDataflowAttributeToDto).ToArray(),
                p.Threats.Select(MapRecommendationToDto).ToArray(),
                p.Images,
                p.Resources
            )).ToArray();
        }

        private static DataflowAttributeDto MapDataflowAttributeToDto(DataflowAttribute dataflowAttribute)
        {
            return new DataflowAttributeDto(
                dataflowAttribute.Number,
                dataflowAttribute.Transport,
                dataflowAttribute.DataClassification,
                dataflowAttribute.Authentication,
                dataflowAttribute.Authorization,
                dataflowAttribute.Notes
            );
        }

        private static RecommendationDto MapRecommendationToDto(Recommendation recommendation)
        {
            return new RecommendationDto(
                recommendation.Id,
                recommendation.Title,
                recommendation.Description,
                recommendation.BenchmarkIds
            );
        }
    }
}
