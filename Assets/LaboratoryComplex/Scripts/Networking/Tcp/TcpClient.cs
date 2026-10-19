using Networking.Request;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Networking
{
    public class TCPClient
    {
        private readonly IMessageSerializer _messageSerializer;

        private Connection _connection;
        private CancellationTokenSource _connectionCts;
        private Task _readTask;

        public TCPClient(IMessageSerializer messageSerializer)
        {
            _messageSerializer = messageSerializer;
        }

        public async Task ConnectAsync(string host, int port, CancellationToken cancellationToken)
        {
            if (_connection != null)
                throw new InvalidOperationException("Client is already connected.");

            var client = new TcpClient();

            try
            {
                await client.ConnectAsync(host, port);
                cancellationToken.ThrowIfCancellationRequested();

                _connection = new Connection(client, _messageSerializer);
                _connection.MessageReceived += OnMessageReceived;

                _connectionCts = new CancellationTokenSource();
                _readTask = RunReadLoopAsync(_connection, _connectionCts.Token);
            }
            catch
            {
                client.Dispose();
                _connection?.Dispose();
                _connection = null;

                _connectionCts?.Dispose();
                _connectionCts = null;

                throw;
            }
        }

        private async Task RunReadLoopAsync(Connection connection, CancellationToken cancellationToken)
        {
            try
            {
                await connection.StartAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                Debug.LogError($"[TCP CLIENT] Read loop failed: {exception}");
            }
        }

        private void OnMessageReceived(MessageEnvelope message)
        {
            Debug.Log($"[TCP CLIENT] Received request: {message.Contract}");
        }

        public async Task<TResponse> SendRequestAsync<TResponse>(IRequest request, CancellationToken cancellationToken)
        {
            if (_connection == null)
                throw new InvalidOperationException("Client is not connected.");

            string payload = _messageSerializer.Serialize(request);

            var message = new MessageEnvelope
            {
                Type = MessageType.Request,
                Contract = request.GetType().Name,
                Payload = _messageSerializer.Serialize(request)
            };

            TResponse response = await _connection.SendRequestAsync<TResponse>(message, cancellationToken);
            return response;
        }

        public async Task DisconnectAsync()
        {
            Connection connection = _connection;
            CancellationTokenSource cts = _connectionCts;
            Task readTask = _readTask;

            _connection = null;
            _connectionCts = null;
            _readTask = null;

            if (cts != null)
                cts.Cancel();

            connection?.Dispose();

            if (readTask != null)
                await readTask;

            cts?.Dispose();
        }
    }
}
