namespace Crisp.Ui.Requests;

public record struct GetThreatModelReportArchiveRequest(
    string Id
) : IHttpRequest;
