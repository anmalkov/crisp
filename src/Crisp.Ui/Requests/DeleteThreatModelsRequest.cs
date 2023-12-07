namespace Crisp.Ui.Requests;

public record struct DeleteThreatModelRequest(
    string Id
) : IHttpRequest;
