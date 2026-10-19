using Networking.Request;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Networking
{
    public class Connection : IDisposable
    {
        public event Action<MessageEnvelope> MessageReceived;
        public event Action<Connection> Disconnected;

        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly MessageFramer _messageFramer;
        private readonly IMessageSerializer _serializer;

        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<MessageEnvelope>> _pendingRequests = new();
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);

        private int _readLoopStarted;
        private int _disposed;
        private int _disconnectedRaised;

        public Connection(TcpClient tcpClient, IMessageSerializer serializer)
        {
            _client = tcpClient ?? throw new ArgumentNullException(nameof(tcpClient));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _stream = _client.GetStream();
            _messageFramer = new MessageFramer();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (Volatile.Read(ref _disposed) != 0)
                throw new ObjectDisposedException(nameof(Connection));

            if (Interlocked.Exchange(ref _readLoopStarted, 1) != 0)
                throw new InvalidOperationException("Connection read loop has already been started.");

            Debug.Log($"[CONNECTION] Read loop started: {_client.Client.RemoteEndPoint}");

            await ReadLoopAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task ReadLoopAsync(CancellationToken cancellationToken)
        {

            while (!cancellationToken.IsCancellationRequested && Volatile.Read(ref _disposed) == 0)
            {
                byte[] data;

                try
                {
                    data = await _messageFramer.ReadAsync(_stream, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    if (Volatile.Read(ref _disposed) == 0 && !cancellationToken.IsCancellationRequested)
                        Debug.LogWarning($"[CONNECTION] Read failed: {exception}");

                    break;
                }

                if (data == null || data.Length == 0)
                {
                    Debug.Log($"[CONNECTION] Remote endpoint disconnected: {_client.Client.RemoteEndPoint}");
                    break;
                }

                MessageEnvelope message;

                try
                {
                    string json = Encoding.UTF8.GetString(data);
                    message = _serializer.Deserialize<MessageEnvelope>(json);

                    if (message == null)
                    {
                        Debug.LogWarning("[CONNECTION] Received an empty message.");
                        continue;
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogError($"[CONNECTION] Message deserialization failed: {exception}");
                    continue;
                }

                Debug.Log($"[CONNECTION] Received: Type={message.Type}, Contract={message.Contract}, ID={message.RequestId}");

                if (message.Type == MessageType.Response)
                {
                    HandleResponse(message);
                    continue;
                }

                try
                {
                    MessageReceived?.Invoke(message);
                }
                catch (Exception exception)
                {
                    Debug.LogError($"[CONNECTION] MessageReceived handler failed: {exception}");
                }
            }
        }

        private void HandleResponse(MessageEnvelope message)
        {
            if (_pendingRequests.TryRemove(message.RequestId, out var completion))
            {
                completion.TrySetResult(message);
                return;
            }

            Debug.LogWarning($"[CONNECTION] No pending request for response ID={message.RequestId}. Pending count: {_pendingRequests.Count}");
        }

        public async Task SendAsync(MessageEnvelope message, CancellationToken cancellationToken)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            ThrowIfDisposed();

            await _sendLock.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                ThrowIfDisposed();

                string json = _serializer.Serialize(message);
                byte[] data = Encoding.UTF8.GetBytes(json);

                await _messageFramer.SendAsync(_stream, data, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _sendLock.Release();
            }
        }

        public async Task<TResponse> SendRequestAsync<TResponse>(MessageEnvelope request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            ThrowIfDisposed();

            request.RequestId = Guid.NewGuid();
            request.Type = MessageType.Request;

            var completion = new TaskCompletionSource<MessageEnvelope>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            if (!_pendingRequests.TryAdd(request.RequestId, completion))
                throw new InvalidOperationException($"Duplicate RequestId: {request.RequestId}");

            using var registration = cancellationToken.Register(() => completion.TrySetCanceled());

            try
            {
                Debug.Log($"[CONNECTION] Sending request: Contract={request.Contract}, ID={request.RequestId}");

                await SendAsync(request, cancellationToken).ConfigureAwait(false);

                MessageEnvelope response = await completion.Task.ConfigureAwait(false);

                if (!string.Equals(response.Contract, typeof(TResponse).Name, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"Unexpected response contract. Expected {typeof(TResponse).Name}, received {response.Contract}.");
                }

                return _serializer.Deserialize<TResponse>(response.Payload);
            }
            finally
            {
                _pendingRequests.TryRemove(request.RequestId, out _);
            }
        }

        private void ThrowIfDisposed()
        {
            if (Volatile.Read(ref _disposed) != 0)
                throw new ObjectDisposedException(nameof(Connection));
        }

        private void FailPendingRequests(Exception exception)
        {
            foreach (var pair in _pendingRequests)
            {
                if (_pendingRequests.TryRemove(pair.Key, out var completion))
                    completion.TrySetException(exception);
            }
        }

        private void RaiseDisconnected()
        {
            if (Interlocked.Exchange(ref _disconnectedRaised, 1) != 0)
                return;

            try
            {
                Disconnected?.Invoke(this);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[CONNECTION] Disconnected handler failed: {exception}");
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            FailPendingRequests(new IOException("Connection was closed before the response was received."));

            try
            {
                _stream.Dispose();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[CONNECTION] Error disposing stream: {exception.Message}");
            }

            try
            {
                _client.Close();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[CONNECTION] Error closing client: {exception.Message}");
            }

            RaiseDisconnected();
        }
    }
}
