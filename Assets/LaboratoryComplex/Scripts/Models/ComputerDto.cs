using System;

public sealed class ComputerDto
{
    public Guid Id { get; }
    public string IpAddress { get; }
    public string HostName { get; }
    public bool IsConnected { get; }

    public ComputerDto(Guid id, string ipAddress, string hostName, bool isConnected)
    {
        Id = id;
        IpAddress = ipAddress;
        HostName = hostName;
        IsConnected = isConnected;
    }
}
