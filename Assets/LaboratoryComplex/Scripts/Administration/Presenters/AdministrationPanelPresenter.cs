using Networking;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

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
            throw new InvalidOperationException("Administration panel is already active.");

        _view.gameObject.SetActive(true);

        if (_serverProvider.Instance != null)
            return;


        _tcpServer = _networkingFactory.Create();
        _serverProvider.Set(_tcpServer);

        _serverCts = new CancellationTokenSource();
        _broadcastCts = new CancellationTokenSource();

        _serverTask = StartServerAsync(_serverCts.Token);

        ServerDiscoveryMessage discoveryMessage = new ServerDiscoveryMessage
        {
            TcpPort = _tcpServer.Port
        };

        string json = _messageSerializer.Serialize(discoveryMessage);

        _broadcastTask = BroadcastLoopAsync(json, _broadcastCts.Token);

        await Task.CompletedTask;
    }

    public async Task HideAsync()
    {
        _view.gameObject.SetActive(false);

        if (_serverCts == null)
            return;

        _broadcastCts.Cancel();
        _serverCts.Cancel();

        _tcpServer?.StopAsync();

        await AwaitTaskAsync(_broadcastTask);
        await AwaitTaskAsync(_serverTask);

        _broadcastCts.Dispose();
        _serverCts.Dispose();

        _broadcastCts = null;
        _serverCts = null;
        _broadcastTask = null;
        _serverTask = null;
        _tcpServer = null;
    }

    private async Task StartServerAsync(CancellationToken cancellationToken)
    {
        try
        {
            Debug.Log($"Starting TCP server on port {_tcpServer.Port}...");
            await _tcpServer.StartAsync(cancellationToken);
            Debug.Log($"Server status {_tcpServer.Started}");
        }
        catch (OperationCanceledException)
        {
            Debug.Log("TCP server stopped.");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private async Task BroadcastLoopAsync(string json, CancellationToken cancellationToken)
    {
        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                //Debug.Log("BROADCAST");
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