using Networking;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class UdpDiscoveryListener : IDisposable
{
    private readonly UdpClient _udpClient;
    private IMessageSerializer _messageSerializer;
    private readonly int _discoveryPort;

    public UdpDiscoveryListener(IMessageSerializer messageSerializer, NetworkingConfig config)
    {
        _messageSerializer = messageSerializer;
        _discoveryPort = config.udpDiscovery.port;
        _udpClient = new UdpClient
        {
            EnableBroadcast = true
        };

        _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, _discoveryPort));
    }

    public async Task<string> WaitForServer()
    {
        while (true)
        {
            UdpReceiveResult result = await _udpClient.ReceiveAsync();
            string data = Encoding.UTF8.GetString(result.Buffer);

            return data;
        }
    }

    public async Task<DiscoveredServer> WaitForServerAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            UdpReceiveResult result = await _udpClient.ReceiveAsync();

            var message = Encoding.UTF8.GetString(result.Buffer);

            Debug.Log(message);
            var discovery = _messageSerializer.Deserialize<ServerDiscoveryMessage>(message);

            Debug.Log($"Received discovery message from {discovery}");
            if (discovery == null)
            {
                continue;
            }

            DiscoveredServer discoveredServer = new DiscoveredServer
            {
                IpAddress = result.RemoteEndPoint.Address,
                TcpPort = discovery.TcpPort
            };


            return discoveredServer;
        }
    }

    public void Dispose()
    {
        _udpClient.Dispose();
    }
}
