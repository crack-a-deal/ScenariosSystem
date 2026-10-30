using Networking;
using Networking.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

/// <summary>
/// Дебаг-оверлей сети поверх экрана (OnGUI).
/// F8 — показать/скрыть. Провайдеры подтягиваются через Zenject
/// (см. биндинг в NetworkingInstaller) либо вручную:
/// NetworkDebugOverlay.EnsureExists() + RegisterServer/RegisterClient.
/// </summary>
public class NetworkDebugOverlay : MonoBehaviour
{
    private const float SnapshotInterval = 0.25f;

    // Должно совпадать с TcpServer.PendingRequestTtl (90 секунд).
    private const float PendingTtlSeconds = 90f;

    private const int WindowId = 51001;

    [Header("Управление")]
    public KeyCode toggleKey = KeyCode.F8;
    public bool showOnStart = true;

    [Inject] private TcpServerProvider _serverProvider;
    [Inject] private TcpClientProvider _clientProvider;

    private static NetworkDebugOverlay _instance;
    private static TcpServer _manualServer;
    private static TCPClient _manualClient;

    private bool _visible;
    private Rect _windowRect = new Rect(16, 16, 760, 560);
    private Vector2 _scroll;
    private int _tab;
    private float _fps;

    private static readonly string[] Tabs = { "Сводка", "Клиенты", "Запросы", "Хендлеры", "Лог" };

    // ---- кэш снимка: OnGUI только рисует, данные собираются 4 раза/сек ----
    private float _snapshotTimer = -1f;
    private TcpServer _server;
    private TCPClient _client;
    private NetworkTelemetry.LatencyInfo _latency;
    private (ConnectedClient client, NetworkTelemetry.EndpointStats stats, int pending)[] _clientRows = Array.Empty<(ConnectedClient, NetworkTelemetry.EndpointStats, int)>();
    private (PendingRequest request, float ageSeconds)[] _pendingRows = Array.Empty<(PendingRequest, float)>();
    private (string contract, NetworkTelemetry.HandlerStats stats)[] _handlerRows = Array.Empty<(string, NetworkTelemetry.HandlerStats)>();
    private (string key, NetworkTelemetry.EndpointStats stats)[] _endpointRows = Array.Empty<(string, NetworkTelemetry.EndpointStats)>();
    private readonly List<NetworkTelemetry.LogEvent> _logRows = new List<NetworkTelemetry.LogEvent>();

    // ---- статический доступ без инсталлера ----

    /// <summary>Создаёт оверлей, если его ещё нет. Можно звать откуда угодно.</summary>
    public static NetworkDebugOverlay EnsureExists(bool visible = true)
    {
        if (_instance != null)
        {
            if (visible)
                _instance._visible = true;
            return _instance;
        }

        var gameObject = new GameObject("NetworkDebugOverlay (Debug)");
        DontDestroyOnLoad(gameObject);
        _instance = gameObject.AddComponent<NetworkDebugOverlay>();
        _instance._visible = visible;
        return _instance;
    }

    /// <summary>Ручная регистрация сервера, если Zenject-биндинг не используется.</summary>
    public static void RegisterServer(TcpServer server) => _manualServer = server;

    /// <summary>Ручная регистрация клиента, если Zenject-биндинг не используется.</summary>
    public static void RegisterClient(TCPClient client) => _manualClient = client;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        _visible = showOnStart;
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            _visible = !_visible;

        _fps = 1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f);

        if (!_visible)
            return;

        _snapshotTimer -= Time.unscaledDeltaTime;

        if (_snapshotTimer <= 0f)
        {
            _snapshotTimer = SnapshotInterval;
            TakeSnapshot();
        }
    }

    private void TakeSnapshot()
    {
        _server = _serverProvider != null ? _serverProvider.Instance : _manualServer;
        _client = _clientProvider != null ? _clientProvider.Instance : _manualClient;

        _latency = NetworkTelemetry.GetLatency();

        if (_server != null)
        {
            Dictionary<Guid, int> pendingByClient = _server.PendingRequests
                .GroupBy(request => request.ClientId)
                .ToDictionary(group => group.Key, group => group.Count());

            _clientRows = _server.ClientRegistry.Clients
                .Select(client =>
                {
                    pendingByClient.TryGetValue(client.Id, out int pending);
                    return (client, FindStats(client.IpAddress), pending);
                })
                .ToArray();

            _pendingRows = _server.PendingRequests
                .Select(request => (request, (float)(DateTime.UtcNow - request.ReceivedAtUtc).TotalSeconds))
                .OrderByDescending(row => row.Item2)
                .ToArray();
        }
        else
        {
            _clientRows = Array.Empty<(ConnectedClient, NetworkTelemetry.EndpointStats, int)>();
            _pendingRows = Array.Empty<(PendingRequest, float)>();
        }

        _handlerRows = NetworkTelemetry.Handlers
            .OrderByDescending(pair => pair.Value.TotalMs)
            .Select(pair => (pair.Key, pair.Value))
            .ToArray();

        _endpointRows = NetworkTelemetry.Endpoints
            .OrderByDescending(pair => pair.Value.LastActivityUtc)
            .Select(pair => (pair.Key, pair.Value))
            .ToArray();

        _logRows.Clear();
        _logRows.AddRange(NetworkTelemetry.GetRecentLog(120));
    }

    /// <summary>Endpoint-статистика по IP клиента (ключ телеметрии — «ip:port»).</summary>
    private static NetworkTelemetry.EndpointStats FindStats(string ipAddress)
    {
        foreach (var pair in NetworkTelemetry.Endpoints)
        {
            if (pair.Key.StartsWith(ipAddress + ":", StringComparison.Ordinal))
                return pair.Value;
        }

        return null;
    }

    // ================== ОТОБРАЖЕНИЕ ==================

    private void OnGUI()
    {
        if (!_visible)
            return;

        _windowRect = GUILayout.Window(WindowId, _windowRect, DrawWindow, $"Сеть ▸ Дебаг   [{toggleKey}]");
    }

    private void DrawWindow(int windowId)
    {
        GUILayout.BeginHorizontal();

        GUILayout.Label($"FPS {(int)_fps}", Width(60));
        GUILayout.Space(8);

        if (_server != null && _server.Started)
            Label($"Сервер ● :{_server.Port}", Color.green, 110);
        else
            Label("Сервер ○", new Color(0.6f, 0.6f, 0.6f), 110);

        GUILayout.Space(6);
        Label($"Клиентов: {_clientRows.Length}", Color.white, 95);
        Label($"Ждут ответа: {_pendingRows.Length}", _pendingRows.Length > 0 ? Color.yellow : Color.white, 110);
        Label($"В обработке: {NetworkTelemetry.ActiveHandlers}", NetworkTelemetry.ActiveHandlers > 0 ? Color.yellow : Color.white, 115);

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Сброс", Width(60)))
            NetworkTelemetry.Reset();

        GUILayout.EndHorizontal();

        _tab = GUILayout.Toolbar(_tab, Tabs);
        _scroll = GUILayout.BeginScrollView(_scroll, false, true);

        switch (_tab)
        {
            case 0: DrawSummary(); break;
            case 1: DrawClients(); break;
            case 2: DrawPending(); break;
            case 3: DrawHandlers(); break;
            case 4: DrawLog(); break;
        }

        GUILayout.EndScrollView();
        GUI.DragWindow(new Rect(0, 0, 10000, 24));
    }

    // ---- вкладка «Сводка» ----
    private void DrawSummary()
    {
        Section("Этот ПК");
        Row2("Роль", _server != null && _server.Started ? "преподаватель (сервер работает)" : "сервер не запущен");
        //Row2("TCP-клиент", _client != null && _client.IsConnected ? "подключён" : "не подключён");

        Section("Трафик (с момента запуска)");
        Row2("↓ получено", $"{FmtBytes(NetworkTelemetry.BytesReceived)} ({NetworkTelemetry.MessagesReceived} сообщений)");
        Row2("↑ отправлено", $"{FmtBytes(NetworkTelemetry.BytesSent)} ({NetworkTelemetry.MessagesSent} сообщений)");
        Row2("Подключений / обрывов", $"{NetworkTelemetry.ConnectCount} / {NetworkTelemetry.DisconnectCount}");
        Row2("Аптайм", $"{DateTime.UtcNow - NetworkTelemetry.StartUtc:hh\\:mm\\:ss}");

        Section("Запросы (этот ПК как клиент)");
        Row2("Отправлено / завершено", $"{NetworkTelemetry.RequestsSent} / {NetworkTelemetry.RequestsCompleted}");
        Row2("Ошибки / таймауты", $"{NetworkTelemetry.RequestErrors} / {NetworkTelemetry.RequestTimeouts}");

        if (_latency.Count > 0)
            Row2("Задержка (посл. / ср. / макс)", $"{_latency.Last:F0} / {_latency.Average:F0} / {_latency.Max:F0} мс");

        Section("Хендлеры (этот ПК как сервер)");
        long totalCount = 0, totalErrors = 0;
        foreach (var row in _handlerRows) { totalCount += row.stats.Count; totalErrors += row.stats.Errors; }
        Row2("Всего обработано / ошибок", $"{totalCount} / {totalErrors}");

        Section("Соединения (endpoint'ы)");
        Row(("endpoint", 210), ("состояние", 90), ("↓", 90), ("↑", 90), ("сообщ. ↓/↑", 110), ("запросов", 80), ("активность", 90));

        if (_endpointRows.Length == 0)
        {
            Label("Соединений ещё не было", new Color(0.6f, 0.6f, 0.6f));
            return;
        }

        foreach (var row in _endpointRows)
        {
            NetworkTelemetry.EndpointStats stats = row.stats;

            GUILayout.BeginHorizontal();
            Label(row.key, stats.Connected ? Color.white : new Color(0.6f, 0.6f, 0.6f), 210);
            Label(stats.Connected ? "● живое" : "✖ закрыто", stats.Connected ? Color.green : Color.red, 90);
            GUILayout.Label(FmtBytes(stats.BytesIn), Width(90));
            GUILayout.Label(FmtBytes(stats.BytesOut), Width(90));
            GUILayout.Label($"{stats.MsgIn}/{stats.MsgOut}", Width(110));
            GUILayout.Label(stats.RequestsIn.ToString(), Width(80));
            GUILayout.Label(Ago(stats.LastActivityUtc), Width(90));
            GUILayout.EndHorizontal();
        }
    }

    // ---- вкладка «Клиенты» ----
    private void DrawClients()
    {
        if (_server == null)
        {
            Label("Сервер не запущен — таблица доступна на машине преподавателя.", new Color(0.6f, 0.6f, 0.6f));
            return;
        }

        Row(("id", 80), ("ip", 130), ("имя ПК", 150), ("доступ", 60), ("↓ / ↑", 150), ("запросов", 80), ("ждут ответа", 90), ("активность", 90));

        if (_clientRows.Length == 0)
        {
            Label("Подключённых клиентов нет", new Color(0.6f, 0.6f, 0.6f));
            return;
        }

        foreach (var row in _clientRows)
        {
            ConnectedClient client = row.client;
            NetworkTelemetry.EndpointStats stats = row.stats;

            GUILayout.BeginHorizontal();
            Label(client.Id.ToString("N").Substring(0, 8), new Color(0.7f, 0.9f, 1f), 80);
            GUILayout.Label(client.IpAddress ?? "—", Width(130));
            GUILayout.Label(client.HostName ?? "—", Width(150));
            Label(client.IsAuthorized ? "✓" : "—", client.IsAuthorized ? Color.green : Color.yellow, 60);
            GUILayout.Label(stats != null ? $"{FmtBytes(stats.BytesIn)} / {FmtBytes(stats.BytesOut)}" : "—", Width(150));
            GUILayout.Label(stats != null ? stats.RequestsIn.ToString() : "—", Width(80));
            //GUILayout.Label(row.pending > 0 ? row.pending.ToString() : "—", row.pending > 0 ? Color.yellow : Color.white, 90);
            GUILayout.Label(stats != null ? Ago(stats.LastActivityUtc) : "—", Width(90));
            GUILayout.EndHorizontal();
        }
    }

    // ---- вкладка «Запросы» (deferred, ждут ответа преподавателя) ----
    private void DrawPending()
    {
        if (_server == null)
        {
            Label("Сервер не запущен — таблица доступна на машине преподавателя.", new Color(0.6f, 0.6f, 0.6f));
            return;
        }

        if (_pendingRows.Length == 0)
        {
            Label("Нет запросов, ожидающих ответа преподавателя", Color.green);
            return;
        }

        Row(("id", 80), ("от (ip)", 150), ("имя ПК", 150), ("контракт", 130), ("возраст", 90), ("до авто-отказа (TTL)", 160));

        foreach (var row in _pendingRows)
        {
            PendingRequest request = row.request;
            float age = row.ageSeconds;
            float ttlLeft = Mathf.Max(0f, PendingTtlSeconds - age);

            Color barColor = Color.green;
            if (age > PendingTtlSeconds * 0.5f) barColor = Color.yellow;
            if (age > PendingTtlSeconds * 0.75f) barColor = Color.red;

            GUILayout.BeginHorizontal();
            Label(request.RequestId.ToString("N").Substring(0, 8), new Color(0.7f, 0.9f, 1f), 80);
            GUILayout.Label(request.IpAddress ?? "—", Width(150));
            GUILayout.Label(request.HostName ?? "—", Width(150));
            GUILayout.Label(request.Contract, Width(130));
            GUILayout.Label($"{age:F0} с", Width(90));
            Rect bar = GUILayoutUtility.GetRect(110, 16);
            DrawBar(bar, ttlLeft / PendingTtlSeconds, barColor);
            //GUILayout.Label($"{ttlLeft:F0} с", ttlLeft < 15f ? Color.red : Color.white, 50);
            GUILayout.EndHorizontal();
        }
    }

    // ---- вкладка «Хендлеры» (сколько и как долго выполняются на сервере) ----
    private void DrawHandlers()
    {
        Row(("контракт", 250), ("вызовов", 80), ("ошибок", 70), ("средн.", 90), ("послед.", 90), ("ожидал семафор", 120), ("активных", 80));

        if (_handlerRows.Length == 0)
        {
            Label("Хендлеры ещё не вызывались", new Color(0.6f, 0.6f, 0.6f));
            return;
        }

        foreach (var row in _handlerRows)
        {
            NetworkTelemetry.HandlerStats stats = row.stats;
            double average = stats.Count > 0 ? stats.TotalMs / stats.Count : 0;

            GUILayout.BeginHorizontal();
            Label(row.contract, stats.Errors > 0 ? Color.red : Color.white, 250);
            GUILayout.Label(stats.Count.ToString(), Width(80));
            Label(stats.Errors.ToString(), stats.Errors > 0 ? Color.red : new Color(0.6f, 0.6f, 0.6f), 70);
            GUILayout.Label($"{average:F0} мс", Width(90));
            GUILayout.Label($"{stats.LastMs:F0} мс", Width(90));
            //GUILayout.Label($"{stats.LastQueueMs:F0} мс", stats.LastQueueMs > 100 ? Color.yellow : Color.white, 120);
            Label(stats.Active.ToString(), stats.Active > 0 ? Color.yellow : new Color(0.6f, 0.6f, 0.6f), 80);
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(6);
        Label("«Ожидал семафор» — время в очереди за доступом к DbContext (см. RequestDispatcher).", new Color(0.6f, 0.6f, 0.6f));
        Label("Если это число большое — контент обрабатывается последовательно, нужен IDbContextFactory.", new Color(0.6f, 0.6f, 0.6f));
    }

    // ---- вкладка «Лог» ----
    private void DrawLog()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label($"{_logRows.Count} событий (кольцо на 256)", Width(250));

        if (GUILayout.Button("Очистить", Width(90)))
            NetworkTelemetry.ClearLog();

        GUILayout.EndHorizontal();

        foreach (NetworkTelemetry.LogEvent logEvent in _logRows)
        {
            Color color;

            if (logEvent.Kind == NetworkTelemetry.EventKind.Error)
                color = new Color(1f, 0.4f, 0.4f);
            else if (logEvent.Kind == NetworkTelemetry.EventKind.Warn)
                color = new Color(1f, 0.85f, 0.3f);
            else
                color = new Color(0.85f, 0.85f, 0.85f);

            Label($"{logEvent.TimeUtc.ToLocalTime():HH:mm:ss}  {logEvent.Message}", color);
        }
    }

    // ================== ХЕЛПЕРЫ ==================

    private static GUILayoutOption Width(float width) => GUILayout.Width(width);

    private static void Label(string text, Color color, float width)
    {
        GUI.color = color;
        GUILayout.Label(text, GUILayout.Width(width));
        GUI.color = Color.white;
    }

    private static void Label(string text, Color color)
    {
        GUI.color = color;
        GUILayout.Label(text);
        GUI.color = Color.white;
    }

    private static void Row(params (string text, float width)[] cells)
    {
        GUILayout.BeginHorizontal();
        foreach (var cell in cells)
            Label(cell.text, new Color(0.75f, 0.75f, 0.75f), cell.width);
        GUILayout.EndHorizontal();
    }

    private static void Row2(string name, string value)
    {
        GUILayout.BeginHorizontal();
        Label(name, new Color(0.75f, 0.75f, 0.75f), 260);
        GUILayout.Label(value);
        GUILayout.EndHorizontal();
    }

    private static void Section(string title)
    {
        GUILayout.Space(8);
        Label(title, new Color(1f, 0.8f, 0.3f));
    }

    private static void DrawBar(Rect rect, float fill, Color color)
    {
        Texture2D texture = Texture2D.whiteTexture;
        Color old = GUI.color;

        GUI.color = new Color(0.15f, 0.15f, 0.15f, 0.7f);
        GUI.DrawTexture(rect, texture);

        if (fill > 0f)
        {
            GUI.color = color;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(fill), rect.height), texture);
        }

        GUI.color = old;
    }

    private static string FmtBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} Б";
        if (bytes < 1024 * 1024) return $"{bytes / 1024f:F1} КБ";
        return $"{bytes / (1024f * 1024f):F1} МБ";
    }

    private static string Ago(DateTime utc)
    {
        double seconds = (DateTime.UtcNow - utc).TotalSeconds;

        if (seconds < 60)
            return $"{seconds:F0} с";

        return $"{seconds / 60:F0} мин";
    }
}