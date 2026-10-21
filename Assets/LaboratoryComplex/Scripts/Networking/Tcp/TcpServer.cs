using Networking.Request;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Networking
{
    public class TcpServer
    {
        public event Action<PendingRequest> AdminActionRequestReceived;
        public event Action<Exception> ErrorOccurred;
        public event Action<Guid> ClientDisconnected;

        private readonly TcpListener _listener;
        private readonly IMessageSerializer _serializer;
        private readonly RequestDispatcher _requestDispatcher;
        private readonly ClientRegistry _clientRegistry;
        private readonly ConcurrentDictionary<Guid, PendingRequest> _pendingRequests = new ConcurrentDictionary<Guid, PendingRequest>();
        private readonly HashSet<string> _deferredContracts = new HashSet<string>();

        private CancellationTokenSource _runCts;
        private bool _started;
        public bool Started => _started;
        public ClientRegistry ClientRegistry => _clientRegistry;

        public IReadOnlyCollection<PendingRequest> PendingRequests => _pendingRequests.Values.ToArray();

        private int _port;
        public int Port => _port;


        private Task _acceptTask;

        public TcpServer(int port, IMessageSerializer serializer, RequestDispatcher requestDispatcher, ClientRegistry clientRegistry)
        {
            _port = port;
            _listener = new TcpListener(IPAddress.Any, port);
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _requestDispatcher = requestDispatcher ?? throw new ArgumentNullException(nameof(requestDispatcher));
            _clientRegistry = clientRegistry ?? throw new ArgumentNullException(nameof(clientRegistry));
        }

        public void RegisterDeferredRequest<TRequest>()
        {
            if (_started)
                throw new InvalidOperationException("Register deferred request types before starting the server.");

            _deferredContracts.Add(typeof(TRequest).Name);
        }

        public IReadOnlyCollection<PendingRequest> GetPendingRequests<TRequest>()
        {
            return _pendingRequests.Values
                .Where(request => request.Contract == typeof(TRequest).Name)
                .ToArray();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_runCts != null || _started)
                throw new InvalidOperationException("Server is already running.");

            cancellationToken.ThrowIfCancellationRequested();

            _runCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            try
            {
                _listener.Start();
                _port = ((IPEndPoint)_listener.LocalEndpoint).Port;
                _started = true;
                _acceptTask = AcceptLoopAsync(_runCts.Token);

                return Task.CompletedTask;
            }
            catch
            {
                _listener.Stop();
                _runCts.Dispose();
                _runCts = null;
                throw;
            }
        }

        public async Task StopAsync()
        {
            if (_runCts == null)
                return;

            CancellationTokenSource cts = _runCts;
            _runCts = null;

            cts.Cancel();

            try
            {
                _listener.Stop();
            }
            catch (SocketException)
            {
            }

            StopConnections();

            if (_acceptTask != null)
                await _acceptTask.ConfigureAwait(false);

            _acceptTask = null;
            _started = false;
            cts.Dispose();
        }

        private async Task AcceptLoopAsync(CancellationToken cancellationToken)
        {
            using (cancellationToken.Register(() =>
            {
                try
                {
                    _listener.Stop();
                }
                catch (SocketException)
                {
                }
            }))
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        TcpClient tcpClient = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
                        RegisterClient(tcpClient, cancellationToken);
                    }
                }
                catch (SocketException) when (cancellationToken.IsCancellationRequested)
                {
                }
                catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
                {
                }
                catch (Exception exception)
                {
                    ReportError(exception);
                }
                finally
                {
                    _listener.Stop();
                    _started = false;
                }
            }
        }

        private void RegisterClient(TcpClient tcpClient, CancellationToken cancellationToken)
        {
            string ipAddress = (tcpClient.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString() ?? "Unknown";
            Connection connection = new Connection(tcpClient, _serializer);
            ConnectedClient client = new ConnectedClient(Guid.NewGuid(), connection, ipAddress, "Unknown");

            try
            {
                _clientRegistry.Add(client);
            }
            catch
            {
                connection.Dispose();
                throw;
            }

            connection.MessageReceived += message =>
            {
                _ = HandleMessageSafelyAsync(client.Id, connection, message, cancellationToken);
            };

            _ = RunConnectionAsync(client, cancellationToken);
        }

        private async Task RunConnectionAsync(ConnectedClient client, CancellationToken cancellationToken)
        {
            try
            {
                await client.Connection.StartAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception exception)
            {
                ReportError(exception);
            }
            finally
            {
                try
                {
                    client.Connection.Dispose();
                }
                catch (Exception exception)
                {
                    ReportError(exception);
                }

                _clientRegistry.Remove(client.Id);
                RemovePendingRequests(client.Id);
                ClientDisconnected?.Invoke(client.Id);
            }
        }

        private async Task HandleMessageSafelyAsync(Guid clientId, Connection connection, MessageEnvelope message, CancellationToken cancellationToken)
        {
            try
            {
                await HandleMessageAsync(clientId, connection, message, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                ReportError(exception);
            }
        }

        private async Task HandleMessageAsync(Guid clientId, Connection connection, MessageEnvelope message, CancellationToken cancellationToken)
        {
            if (message.Type != MessageType.Request)
                return;

            if (message.RequestId == Guid.Empty)
                throw new InvalidOperationException($"Request {message.Contract} has an empty RequestId.");

            if (_deferredContracts.Contains(message.Contract))
            {
                StorePendingRequest(clientId, message);
                return;
            }

            DispatchedResponse dispatchedResponse = await _requestDispatcher.DispatchAsync(message.Contract, message.Payload).ConfigureAwait(false);

            MessageEnvelope responseMessage = new MessageEnvelope
            {
                RequestId = message.RequestId,
                Type = MessageType.Response,
                Contract = dispatchedResponse.Contract,
                Payload = dispatchedResponse.Payload
            };

            Debug.Log($"[SERVER] Handler completed: {responseMessage.Contract}, ID={responseMessage.RequestId}, Payload={responseMessage.Payload}");
            await connection.SendAsync(responseMessage, cancellationToken).ConfigureAwait(false);
            Debug.Log($"[SERVER] Response sent: {responseMessage.Contract}, ID={responseMessage.RequestId}, Payload={responseMessage.Payload}");
        }

        private void StorePendingRequest(Guid clientId, MessageEnvelope message)
        {
            if (!_clientRegistry.TryGet(clientId, out var client))
                throw new InvalidOperationException($"Connected client {clientId} was not found.");

            if (message.Contract == nameof(AccessRequest))
            {
                AccessRequest request = _serializer.Deserialize<AccessRequest>(message.Payload);
                _clientRegistry.UpdateHostName(clientId, request.HostName);
                client = _clientRegistry.TryGet(clientId, out var updatedClient) ? updatedClient : client;
            }

            PendingRequest pendingRequest = new PendingRequest(
                message.RequestId,
                clientId,
                message.Contract,
                message.Payload,
                client.IpAddress,
                client.HostName);

            if (!_pendingRequests.TryAdd(message.RequestId, pendingRequest))
                throw new InvalidOperationException($"Duplicate RequestId: {message.RequestId}");

            AdminActionRequestReceived?.Invoke(pendingRequest);
        }

        public async Task RespondAsync<TResponse>(Guid requestId, TResponse response, CancellationToken cancellationToken)
            where TResponse : IResponse
        {
            if (!_pendingRequests.TryGetValue(requestId, out var pendingRequest))
                throw new KeyNotFoundException($"Pending request {requestId} was not found.");

            if (!pendingRequest.TryBeginResponse())
                throw new InvalidOperationException($"Request {requestId} is already being answered.");

            try
            {
                if (!_clientRegistry.TryGet(pendingRequest.ClientId, out var client))
                    throw new InvalidOperationException($"Client {pendingRequest.ClientId} is no longer connected.");

                MessageEnvelope responseMessage = new MessageEnvelope
                {
                    RequestId = pendingRequest.RequestId,
                    Type = MessageType.Response,
                    Contract = typeof(TResponse).Name,
                    Payload = _serializer.Serialize(response)
                };

                await client.Connection.SendAsync(
                    responseMessage,
                    cancellationToken);

                if (response is AccessResponse accessResponse &&
                    accessResponse.Granted)
                {
                    client.Authorize();
                }

                _pendingRequests.TryRemove(requestId, out _);
            }
            catch
            {
                pendingRequest.CancelResponse();
                throw;
            }
        }

        private void RemovePendingRequests(Guid clientId)
        {
            foreach (var request in _pendingRequests.Values.Where(request => request.ClientId == clientId))
                _pendingRequests.TryRemove(request.RequestId, out _);
        }

        private void StopConnections()
        {
            foreach (var client in _clientRegistry.Clients)
            {
                try
                {
                    client.Connection.Dispose();
                }
                catch (Exception exception)
                {
                    ReportError(exception);
                }
            }
        }

        private void ReportError(Exception exception)
        {
            try
            {
                ErrorOccurred?.Invoke(exception);
            }
            catch
            {
                // Ошибка подписчика не должна ломать сетевой цикл.
            }
        }
    }
}
