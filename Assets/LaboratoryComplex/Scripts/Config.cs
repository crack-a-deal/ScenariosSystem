using System;
using UnityEngine;

[Serializable]
public class NetworkingConfig
{
    public TcpConfig tcp;
    public UdpDiscoveryConfig udpDiscovery;
}

[Serializable]
public class TcpConfig
{
    public int port = 5000;
}

[Serializable]
public class UdpDiscoveryConfig
{
    public int port = 37020;
}

[Serializable]
public class TestsConfig
{
    public string testBankFile;
}