using Networking;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

public class ClientRegistry
{
    private readonly ConcurrentDictionary<Guid, ConnectedClient> _clients = new ConcurrentDictionary<Guid, ConnectedClient>();

    public IReadOnlyCollection<ConnectedClient> Clients => _clients.Values.ToArray();

    public void Add(ConnectedClient client)
    {
        if (!_clients.TryAdd(client.Id, client))
            throw new InvalidOperationException($"Client {client.Id} is already registered.");
    }

    public bool TryGet(Guid clientId, out ConnectedClient client)
    {
        return _clients.TryGetValue(clientId, out client);
    }

    public bool Remove(Guid clientId)
    {
        return _clients.TryRemove(clientId, out _);
    }

    public void UpdateHostName(Guid clientId, string hostName)
    {
        if (!string.IsNullOrWhiteSpace(hostName) && _clients.TryGetValue(clientId, out var client))
            client.HostName = hostName;
    }
}
