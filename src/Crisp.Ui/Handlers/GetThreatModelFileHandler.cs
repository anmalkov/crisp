using Crisp.Core.Models;
using Crisp.Core.Services;
using Crisp.Ui.Requests;
using DocumentFormat.OpenXml.Packaging;
using MediatR;
using System.Security.Cryptography.Xml;
using System.Text;

namespace Crisp.Ui.Handlers
{
    public class GetThreatModelFileHandler : IRequestHandler<GetThreatModelFileRequest, IResult>
    {
        private readonly IThreatModelsService _threatModelsService;

        public GetThreatModelFileHandler(IThreatModelsService threatModelsService)
        {
            _threatModelsService = threatModelsService;
        }

        public async Task<IResult> Handle(GetThreatModelFileRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var mimeType = Path.GetExtension(request.FileName)[1..].ToLower() switch
                {
                    "jpg" or "jpeg" => "image/jpeg",
                    "gif" => "image/gif",
                    _ => "image/png"
                };
                var content = await _threatModelsService.GetReportFileAsync(request.Id, request.FileName);
                return content is not null
                    ? Results.File(content, mimeType, request.FileName)
                    : Results.NotFound();
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }
    }
}
