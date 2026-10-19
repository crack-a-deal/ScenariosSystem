using Networking;
using System;

public class TcpClientProvider
{
    public TCPClient Instance { get; private set; }

    public void Set(TCPClient client)
    {
        if (Instance != null && Instance != client)
            throw new InvalidOperationException("TCP client is already registered.");

        Instance = client;
    }

    public void Clear()
    {
        Instance = null;
    }
}