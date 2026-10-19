using Networking;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class StudentWaitingPresenter
{
    public event Action AccessGranted;
    public event Action AccessDenied;
    public event Action<Exception> ConnectionFailed;

    private readonly StudentWaitingView _view;
    private readonly UdpDiscoveryListener _listener;
    private readonly TcpClientProvider _clientProvider;
    private readonly IMessageSerializer _messageSerializer;
    private TCPClient _tcpClient;

    private CancellationTokenSource _lifetimeCts;
    private Task _connectionTask;

    public StudentWaitingPresenter(StudentWaitingView view, UdpDiscoveryListener listener,TcpClientProvider clientProvider,IMessageSerializer messageSerializer)
    {
        _view = view;
        _listener = listener;

        _clientProvider = clientProvider;
        _messageSerializer = messageSerializer;
    }

    public void Show()
    {
        if (_lifetimeCts != null)
            throw new InvalidOperationException("Student waiting presenter is already active.");

        _view.gameObject.SetActive(true);

        _lifetimeCts = new CancellationTokenSource();
        _connectionTask = ConnectToTeacherAsync(_lifetimeCts.Token);
    }

    public async Task HideAsync()
    {
        _view.gameObject.SetActive(false);

        CancellationTokenSource cts = _lifetimeCts;

        if (cts == null)
            return;

        _lifetimeCts = null;
        cts.Cancel();

        try
        {
            if (_connectionTask != null)
                await _connectionTask;
        }
        finally
        {
            cts.Dispose();
            _connectionTask = null;
        }
    }

    private async Task ConnectToTeacherAsync(CancellationToken cancellationToken)
    {
        try
        {
            Debug.Log("Try to find server");
            DiscoveredServer server = await _listener.WaitForServerAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            Debug.Log($"Server found | {server.IpAddress}:{server.TcpPort}");
            
            _tcpClient = new TCPClient(_messageSerializer);
            _clientProvider.Set(_tcpClient);

            await _tcpClient.ConnectAsync(server.IpAddress.ToString(), server.TcpPort, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            Debug.Log($"Connected to server | {server.IpAddress}:{server.TcpPort}");

            AccessRequest request = new AccessRequest
            {
                HostName = Environment.MachineName
            };

            AccessResponse response = await _tcpClient.SendRequestAsync<AccessResponse>(request, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            if (response.Granted)
                AccessGranted?.Invoke();
            else
                AccessDenied?.Invoke();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Ожидаемая отмена при выходе из состояния.
        }
        catch (Exception exception)
        {
            Debug.LogError($"Student connection flow failed: {exception}");
            ConnectionFailed?.Invoke(exception);
        }
    }
}