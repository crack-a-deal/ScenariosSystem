
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class UdpDiscoveryService : IDisposable
{
    private readonly UdpClient _udpClient;
    private readonly int _discoveryPort;

    public UdpDiscoveryService(NetworkingConfig config)
    {
        _discoveryPort = config.udpDiscovery.port;
        _udpClient = new UdpClient
        {
            EnableBroadcast = true
        };
    }

    public async Task BroadcastServerAsync(string message, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        byte[] data = Encoding.UTF8.GetBytes(message);

        var endpoint = new IPEndPoint(IPAddress.Broadcast, _discoveryPort);

        await _udpClient.SendAsync(data, data.Length, endpoint);
    }

    public void Dispose()
    {
        _udpClient.Dispose();
    }
}
