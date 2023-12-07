using Crisp.Ui.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace Crisp.Ui.Requests;

public record struct GetThreatModelFileRequest(
    string Id,
    string FileName
) : IHttpRequest;
