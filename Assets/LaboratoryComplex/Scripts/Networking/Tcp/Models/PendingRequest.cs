using System;
using System.Threading;

public class PendingRequest
{
    private int _responseState;

    public Guid RequestId { get; }
    public Guid ClientId { get; }
    public string Contract { get; }
    public string Payload { get; }
    public string IpAddress { get; }
    public string HostName { get; }
    public DateTime ReceivedAtUtc { get; }

    public PendingRequest(Guid requestId, Guid clientId, string contract, string payload, string ipAddress, string hostName)
    {
        RequestId = requestId;
        ClientId = clientId;
        Contract = contract;
        Payload = payload;
        IpAddress = ipAddress;
        HostName = hostName;
        ReceivedAtUtc = DateTime.UtcNow;
    }

    public bool TryBeginResponse()
    {
        return Interlocked.CompareExchange(ref _responseState, 1, 0) == 0;
    }

    public void CancelResponse()
    {
        Interlocked.CompareExchange(ref _responseState, 0, 1);
    }
}