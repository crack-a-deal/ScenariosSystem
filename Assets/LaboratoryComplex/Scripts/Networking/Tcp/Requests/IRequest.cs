namespace Networking.Request
{

    public interface IRequest
    {

    }

    public interface IRequest<TResponse> : IRequest where TResponse : IResponse
    {
    }
}
