using Crisp.Core.Models;
using Crisp.Core.Services;
using Crisp.Ui.Requests;
using MediatR;
using System.Security.Cryptography.Xml;

namespace Crisp.Ui.Handlers
{
    public record RecommendationDto(
        string Id,
        string Title,
        string Description
    );

    public record CategoryDto(
        string Id,
        string Name,
        string? Description,
        IEnumerable<CategoryDto>? Children,
        IEnumerable<RecommendationDto>? Recommendations
    );

    public class GetCategoriesHandler : IRequestHandler<GetCategoriesRequest, IResult>
    {
        private readonly ICategoriesService _categoriesService;

        public GetCategoriesHandler(ICategoriesService categoriesService)
        {
            _categoriesService = categoriesService;
        }

        public async Task<IResult> Handle(GetCategoriesRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var category = await _categoriesService.GetAsync();
                return Results.Ok(MapCategoryToDto(category));
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }

        private static CategoryDto? MapCategoryToDto(Category? category)
        {
            if (category is null) {
                return null;
            }

            return new CategoryDto(
                category.Id,
                category.Name,
                category.Description,
                category.Children?.Select(MapCategoryToDto),
                category.Recommendations?.Select(MapRecommendationToDto)
            );
        }

        private static RecommendationDto MapRecommendationToDto(Recommendation recommendation)
        {
            return new RecommendationDto(
                recommendation.Id,
                recommendation.Title,
                recommendation.Description
            );
        }
    }
}
