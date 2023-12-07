using Crisp.Ui.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace Crisp.Ui.Requests;

public record struct UpdateThreatModelRequest(
    string Id,
    [FromBody]
    CreateThreatModelDto Body
) : IHttpRequest;
