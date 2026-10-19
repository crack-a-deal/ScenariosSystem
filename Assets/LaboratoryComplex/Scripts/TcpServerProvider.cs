using Networking;
using System;

public class TcpServerProvider
{
    public TcpServer Instance { get; private set; }

    public void Set(TcpServer server)
    {
        if (Instance != null && Instance != server)
            throw new InvalidOperationException("TCP server is already registered.");

        Instance = server;
    }

    public void Clear()
    {
        Instance = null;
    }
}
