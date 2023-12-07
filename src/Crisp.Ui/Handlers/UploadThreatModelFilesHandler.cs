using Crisp.Core.Models;
using Crisp.Core.Services;
using Crisp.Ui.Requests;
using MediatR;
using System.Security.Cryptography.Xml;

namespace Crisp.Ui.Handlers
{
    public class UploadThreatModelFilesHandler : IRequestHandler<UploadThreatModelFilesRequest, IResult>
    {
        private readonly IThreatModelsService _threatModelsService;

        public UploadThreatModelFilesHandler(IThreatModelsService threatModelsService)
        {
            _threatModelsService = threatModelsService;
        }

        public async Task<IResult> Handle(UploadThreatModelFilesRequest request, CancellationToken cancellationToken)
        {
            try
            {
                foreach (var file in request.Files)
                {
                    using var content = new MemoryStream();
                    file.CopyTo(content);
                    await _threatModelsService.StoreFileForReportAsync(request.Id, file.FileName, content.ToArray());
                }
                return Results.Ok();
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }
    }
}
