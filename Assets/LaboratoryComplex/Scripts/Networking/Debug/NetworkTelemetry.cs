using Networking.Request;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Networking.Diagnostics
{
    /// <summary>
    /// Сборщик сетевой телеметрии для дебаг-оверлея.
    /// Потокобезопасен: long-счётчики через Interlocked, словари Concurrent.
    /// Поля-двойники (мс и т.п.) — приближение, для отладки этого достаточно.
    /// </summary>
    public static class NetworkTelemetry
    {
        public enum EventKind { Info, Warn, Error }

        public sealed class LogEvent
        {
            public DateTime TimeUtc;
            public EventKind Kind;
            public string Message;
        }

        public sealed class EndpointStats
        {
            public long BytesIn;
            public long BytesOut;
            public int MsgIn;
            public int MsgOut;
            public int RequestsIn;
            public DateTime LastActivityUtc;
            public DateTime ConnectedAtUtc;
            public bool Connected;
        }

        public sealed class HandlerStats
        {
            public long Count;
            public long Errors;
            public double TotalMs;
            public double LastMs;
            public double LastQueueMs;
            public double TotalQueueMs;
            public int Active;
        }

        public readonly struct LatencyInfo
        {
            public readonly int Count;
            public readonly double Last;
            public readonly double Average;
            public readonly double Max;

            public LatencyInfo(int count, double last, double average, double max)
            {
                Count = count;
                Last = last;
                Average = average;
                Max = max;
            }
        }

        private const int LatencyCapacity = 64;
        private const int LogCapacity = 256;

        private static readonly ConcurrentDictionary<string, EndpointStats> _endpoints = new();
        private static readonly ConcurrentDictionary<string, HandlerStats> _handlers = new();

        private static readonly double[] _latencies = new double[LatencyCapacity];
        private static int _latencyCount;
        private static readonly object _latencyLock = new();

        private static readonly object _logLock = new();
        private static readonly LogEvent[] _log = new LogEvent[LogCapacity];
        private static int _logNext;
        private static int _logCount;

        private static long _bytesSent, _bytesReceived;
        private static long _messagesSent, _messagesReceived;
        private static long _requestsSent, _requestsCompleted, _requestErrors, _requestTimeouts;
        private static long _connects, _disconnects;
        private static int _activeHandlers;

        public static DateTime StartUtc { get; } = DateTime.UtcNow;

        // ---- Счётчики (читает оверлей) ----
        public static long BytesSent => Interlocked.Read(ref _bytesSent);
        public static long BytesReceived => Interlocked.Read(ref _bytesReceived);
        public static long MessagesSent => Interlocked.Read(ref _messagesSent);
        public static long MessagesReceived => Interlocked.Read(ref _messagesReceived);
        public static long RequestsSent => Interlocked.Read(ref _requestsSent);
        public static long RequestsCompleted => Interlocked.Read(ref _requestsCompleted);
        public static long RequestErrors => Interlocked.Read(ref _requestErrors);
        public static long RequestTimeouts => Interlocked.Read(ref _requestTimeouts);
        public static long ConnectCount => Interlocked.Read(ref _connects);
        public static long DisconnectCount => Interlocked.Read(ref _disconnects);
        public static int ActiveHandlers => Volatile.Read(ref _activeHandlers);

        public static IReadOnlyDictionary<string, EndpointStats> Endpoints => _endpoints;
        public static IReadOnlyDictionary<string, HandlerStats> Handlers => _handlers;

        // ---- Соединения ----

        public static void OnConnected(string endpoint)
        {
            Interlocked.Increment(ref _connects);

            EndpointStats stats = _endpoints.GetOrAdd(endpoint, _ => new EndpointStats());
            stats.Connected = true;
            stats.ConnectedAtUtc = DateTime.UtcNow;
            stats.LastActivityUtc = DateTime.UtcNow;

            Log(EventKind.Info, $"TCP ▸ подключение {endpoint}");
        }

        public static void OnDisconnected(string endpoint)
        {
            Interlocked.Increment(ref _disconnects);

            if (_endpoints.TryGetValue(endpoint, out EndpointStats stats))
                stats.Connected = false;

            Log(EventKind.Warn, $"TCP ✖ соединение закрыто {endpoint}");
        }

        // ---- Трафик (вызывается на каждое сообщение) ----

        public static void OnSent(string endpoint, MessageEnvelope message, int bytes)
        {
            Interlocked.Increment(ref _messagesSent);
            Interlocked.Add(ref _bytesSent, bytes);

            if (_endpoints.TryGetValue(endpoint, out EndpointStats stats))
            {
                Interlocked.Add(ref stats.BytesOut, bytes);
                Interlocked.Increment(ref stats.MsgOut);
                stats.LastActivityUtc = DateTime.UtcNow;
            }
        }

        public static void OnReceived(string endpoint, MessageEnvelope message, int bytes)
        {
            Interlocked.Increment(ref _messagesReceived);
            Interlocked.Add(ref _bytesReceived, bytes);

            if (_endpoints.TryGetValue(endpoint, out EndpointStats stats))
            {
                Interlocked.Add(ref stats.BytesIn, bytes);
                Interlocked.Increment(ref stats.MsgIn);
                stats.LastActivityUtc = DateTime.UtcNow;

                if (message.Type == MessageType.Request)
                    Interlocked.Increment(ref stats.RequestsIn);
            }

            // heartbeat-«Ping» в лог не пишем — иначе шум каждые 10 секунд
            if (message.Type == MessageType.Request && message.Contract != "Ping")
                Log(EventKind.Info, $"▸ {message.Contract} от {endpoint}");
            //else if (message.Type == MessageType.Error)
            //    Log(EventKind.Warn, $"✖ сервер вернул ошибку: {message.Payload} ({endpoint})");
        }

        // ---- Запросы (клиентская сторона, Connection.SendRequestAsync) ----

        public static long BeginRequest(string contract)
        {
            Interlocked.Increment(ref _requestsSent);
            return Stopwatch.GetTimestamp();
        }

        public static void EndRequest(string contract, long startedTimestamp, bool success)
        {
            double ms = (Stopwatch.GetTimestamp() - startedTimestamp) * 1000.0 / Stopwatch.Frequency;

            Interlocked.Increment(ref _requestsCompleted);

            lock (_latencyLock)
            {
                _latencies[_latencyCount % LatencyCapacity] = ms;
                _latencyCount++;
            }

            if (!success)
                Interlocked.Increment(ref _requestErrors);

            Log(success ? EventKind.Info : EventKind.Warn,
                success ? $"◂ {contract} — {ms:F0} мс"
                        : $"✖ {contract} — провал через {ms:F0} мс");
        }

        public static void OnTimeout(string contract)
        {
            Interlocked.Increment(ref _requestTimeouts);
            Log(EventKind.Error, $"⏱ таймаут запроса {contract}");
        }

        public static LatencyInfo GetLatency()
        {
            lock (_latencyLock)
            {
                int count = Math.Min(_latencyCount, LatencyCapacity);
                if (count == 0)
                    return default;

                double last = _latencies[(_latencyCount - 1) % LatencyCapacity];
                double sum = 0, max = 0;

                for (int i = 0; i < count; i++)
                {
                    double value = _latencies[(_latencyCount - 1 - i) % LatencyCapacity];
                    sum += value;
                    if (value > max)
                        max = value;
                }

                return new LatencyInfo(count, last, sum / count, max);
            }
        }

        // ---- Хендлеры (серверная сторона, RequestDispatcher) ----

        public static void OnHandlerStarted(string contract, double queueMs)
        {
            Interlocked.Increment(ref _activeHandlers);

            HandlerStats stats = _handlers.GetOrAdd(contract, _ => new HandlerStats());
            Interlocked.Increment(ref stats.Active);
            stats.LastQueueMs = queueMs;
            stats.TotalQueueMs += queueMs;
        }

        public static void OnHandlerCompleted(string contract, double totalMs, Exception exception)
        {
            Interlocked.Decrement(ref _activeHandlers);

            if (!_handlers.TryGetValue(contract, out HandlerStats stats))
                return;

            Interlocked.Decrement(ref stats.Active);
            Interlocked.Increment(ref stats.Count);
            stats.LastMs = totalMs;
            stats.TotalMs += totalMs;

            if (exception != null)
            {
                Interlocked.Increment(ref stats.Errors);
                Log(EventKind.Error, $"{contract} ✖ {exception.GetType().Name}: {exception.Message}");
            }
        }

        // ---- Лог событий ----

        public static void Log(EventKind kind, string message)
        {
            lock (_logLock)
            {
                _log[_logNext % LogCapacity] = new LogEvent
                {
                    TimeUtc = DateTime.UtcNow,
                    Kind = kind,
                    Message = message
                };
                _logNext++;
                if (_logCount < LogCapacity)
                    _logCount++;
            }
        }

        /// <summary>Последние события, новейшие первыми.</summary>
        public static List<LogEvent> GetRecentLog(int maxCount)
        {
            var result = new List<LogEvent>(Math.Min(maxCount, LogCapacity));

            lock (_logLock)
            {
                for (int i = 0; i < _logCount && i < maxCount; i++)
                {
                    int index = ((_logNext - 1 - i) % LogCapacity + LogCapacity) % LogCapacity;
                    result.Add(_log[index]);
                }
            }

            return result;
        }

        public static void ClearLog()
        {
            lock (_logLock)
            {
                _logNext = 0;
                _logCount = 0;
            }
        }

        public static void Reset()
        {
            Interlocked.Exchange(ref _bytesSent, 0);
            Interlocked.Exchange(ref _bytesReceived, 0);
            Interlocked.Exchange(ref _messagesSent, 0);
            Interlocked.Exchange(ref _messagesReceived, 0);
            Interlocked.Exchange(ref _requestsSent, 0);
            Interlocked.Exchange(ref _requestsCompleted, 0);
            Interlocked.Exchange(ref _requestErrors, 0);
            Interlocked.Exchange(ref _requestTimeouts, 0);
            Interlocked.Exchange(ref _connects, 0);
            Interlocked.Exchange(ref _disconnects, 0);
            Interlocked.Exchange(ref _activeHandlers, 0);

            _endpoints.Clear();
            _handlers.Clear();

            lock (_latencyLock)
                _latencyCount = 0;

            ClearLog();
        }
    }
}