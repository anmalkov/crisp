using Crisp.Ui.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Crisp.Ui.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication MediateGet<TRequest>(this WebApplication app, string template)
        where TRequest : IHttpRequest
    {
        app.MapGet(template, async (IMediator mediator, [AsParameters] TRequest request) => await mediator.Send(request));
        return app;
    }

    public static WebApplication MediatePost<TRequest>(this WebApplication app, string template)
        where TRequest : IHttpRequest
    {
        app.MapPost(template, async (IMediator mediator, [AsParameters] TRequest request) => await mediator.Send(request));
        return app;

    }

    public static WebApplication MediatePut<TRequest>(this WebApplication app, string template)
        where TRequest : IHttpRequest
    {
        app.MapPut(template, async (IMediator mediator, [AsParameters] TRequest request) => await mediator.Send(request));
        return app;
    }

    public static WebApplication MediateDelete<TRequest>(this WebApplication app, string template)
        where TRequest : IHttpRequest
    {
        app.MapDelete(template, async (IMediator mediator, [AsParameters] TRequest request) => await mediator.Send(request));
        return app;
    }
}
