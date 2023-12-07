namespace Crisp.Ui.Requests;

public record struct GetThreatModelReportRequest(
    string Id
) : IHttpRequest;
