using Crisp.Ui.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace Crisp.Ui.Requests;

public record struct UploadThreatModelFilesRequest(
    string Id,
    [FromForm]
    IFormFileCollection Files
) : IHttpRequest;
