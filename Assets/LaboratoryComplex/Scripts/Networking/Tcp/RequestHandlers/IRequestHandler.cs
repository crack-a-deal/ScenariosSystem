using System.Threading.Tasks;

namespace Networking.Request
{
    public interface IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IResponse
    {
        Task<TResponse> HandleAsync(TRequest request);
    }
}
