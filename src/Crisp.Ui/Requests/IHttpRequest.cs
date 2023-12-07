using MediatR;

namespace Crisp.Ui.Requests;

public interface IHttpRequest : IRequest<IResult> { }