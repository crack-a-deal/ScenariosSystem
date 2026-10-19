using Networking.Request;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Networking
{

    public class RequestDispatcher
    {
        private readonly IMessageSerializer _serializer;
        private readonly Dictionary<string, Func<string, Task<DispatchedResponse>>> _handlers = new();

        public RequestDispatcher(IMessageSerializer serializer)
        {
            _serializer = serializer;
        }

        public void RegisterHandler<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> handler)
            where TRequest : IRequest<TResponse>
            where TResponse : IResponse
        {
            _handlers[typeof(TRequest).Name] = async payload =>
            {
                TRequest request = _serializer.Deserialize<TRequest>(payload);

                TResponse response = await handler.HandleAsync(request).ConfigureAwait(false);

                return new DispatchedResponse(
                    typeof(TResponse).Name,
                    _serializer.Serialize(response));
            };
        }

        public async Task<DispatchedResponse> DispatchAsync(string contract, string payload)
        {
            if (!_handlers.TryGetValue(contract, out var handler))
            {
                throw new InvalidOperationException(
                    $"No handler registered for {contract}");
            }

            return await handler(payload).ConfigureAwait(false);
        }
    }
}
