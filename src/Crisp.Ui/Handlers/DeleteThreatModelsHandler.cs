using Crisp.Core.Models;
using Crisp.Core.Services;
using Crisp.Ui.Requests;
using MediatR;
using System.Security.Cryptography.Xml;

namespace Crisp.Ui.Handlers
{
    public class DeleteThreatModelsHandler : IRequestHandler<DeleteThreatModelRequest, IResult>
    {
        private readonly IThreatModelsService _threatModelsService;

        public DeleteThreatModelsHandler(IThreatModelsService threatModelsService)
        {
            _threatModelsService = threatModelsService;
        }

        public async Task<IResult> Handle(DeleteThreatModelRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _threatModelsService.DeleteAsync(request.Id);
                return Results.Ok();
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }
    }
}
