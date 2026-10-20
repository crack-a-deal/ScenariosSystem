using Networking;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class AdministrationPanelPresenter : IDisposable
{
    private const int BROADCAST_INTERVAL = 2000;

    public event Action BackClicked;

    private readonly AdministrationPanelView _view;
    private readonly TeacherNetworkingFactory _networkingFactory;
    private readonly UdpDiscoveryService _discoveryService;
    private readonly IMessageSerializer _messageSerializer;
    private readonly AdminTablesPresenter _adminTables;
    private TcpServer _tcpServer;

    private CancellationTokenSource _broadcastCts;
    private CancellationTokenSource _serverCts;

    private Task _broadcastTask;
    private Task _serverTask;

    private readonly TcpServerProvider _serverProvider;

    public AdministrationPanelPresenter(AdministrationPanelView view, TeacherNetworkingFactory networkingFactory, UdpDiscoveryService discoveryService, IMessageSerializer messageSerializer, AdminTablesPresenter adminTables, TcpServerProvider serverProvider)
    {
        _view = view;
        _networkingFactory = networkingFactory;
        _discoveryService = discoveryService;
        _messageSerializer = messageSerializer;
        _adminTables = adminTables;

        _serverProvider = serverProvider;

        _view.BackClicked += OnBackClicked;
    }

    public async void Dispose()
    {
        if (_tcpServer != null)
            await _tcpServer.StopAsync();
    }

    public async Task ShowAsync()
    {
        if (_serverCts != null)
            throw new InvalidOperationException(
                "Administration panel is already active.");

        _view.gameObject.SetActive(true);

        TcpServer existingServer = _serverProvider.Instance;

        if (existingServer != null)
        {
            if (existingServer.Started)
                return;

            await existingServer.StopAsync();
            _serverProvider.Clear();
        }

        _tcpServer = _networkingFactory.Create();
        _serverCts = new CancellationTokenSource();

        try
        {
            Debug.Log($"Starting TCP server on port {_tcpServer.Port}...");

            await _tcpServer.StartAsync(_serverCts.Token);
            Debug.Log($"Server status {_tcpServer.Started} {_tcpServer.Port}");

            _serverProvider.Set(_tcpServer);

            ServerDiscoveryMessage discoveryMessage =
                new ServerDiscoveryMessage
                {
                    TcpPort = _tcpServer.Port
                };

            string json = _messageSerializer.Serialize(discoveryMessage);

            _broadcastCts = new CancellationTokenSource();

            _broadcastTask = BroadcastLoopAsync(
                json,
                _broadcastCts.Token);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);

            _serverCts.Cancel();

            await _tcpServer.StopAsync();

            _serverProvider.Clear();

            _tcpServer = null;

            _serverCts.Dispose();
            _serverCts = null;

            throw;
        }
    }

    public async Task HideAsync()
    {
        _view.gameObject.SetActive(false);

        TcpServer server = _tcpServer;

        _broadcastCts?.Cancel();
        _serverCts?.Cancel();

        try
        {
            if (server != null)
                await server.StopAsync();

            await AwaitTaskAsync(_broadcastTask);
        }
        finally
        {
            if (server != null)
                _serverProvider.Clear();

            _broadcastCts?.Dispose();
            _serverCts?.Dispose();

            _broadcastCts = null;
            _serverCts = null;
            _broadcastTask = null;
            _tcpServer = null;
        }
    }

    private async Task BroadcastLoopAsync(string json, CancellationToken cancellationToken)
    {
        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Debug.Log("BROADCAST");
                await _discoveryService.BroadcastServerAsync(json, cancellationToken);
                await Task.Delay(BROADCAST_INTERVAL, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Broadcast loop stopped.");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private static async Task AwaitTaskAsync(Task task)
    {
        if (task == null)
            return;

        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void OnBackClicked()
    {
        BackClicked?.Invoke();
    }
}