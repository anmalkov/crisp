using Crisp.Core.Models;
using Crisp.Core.Services;
using Crisp.Ui.Requests;
using MediatR;
using System.Security.Cryptography.Xml;
using System.Text;

namespace Crisp.Ui.Handlers
{
    public class GetThreatModelReportHandler : IRequestHandler<GetThreatModelReportRequest, IResult>
    {
        private readonly IThreatModelsService _threatModelsService;

        public GetThreatModelReportHandler(IThreatModelsService threatModelsService)
        {
            _threatModelsService = threatModelsService;
        }

        public async Task<IResult> Handle(GetThreatModelReportRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var report = await _threatModelsService.GetReportAsync(request.Id);
                return report is not null
                    ? Results.File(Encoding.UTF8.GetBytes(report), "text/markdown", "security-plan.md")
                    : Results.NotFound();
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }

        }
    }
}
