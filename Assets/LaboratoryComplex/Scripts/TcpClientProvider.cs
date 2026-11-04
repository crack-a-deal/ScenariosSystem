using Networking;
using System;
using System.Threading.Tasks;

public class TcpClientProvider
{
    public TCPClient Instance { get; private set; }

    public void Set(TCPClient client)
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));

        TCPClient previous = Instance;

        if (ReferenceEquals(previous, client))
            return;

        Instance = client;

        if (previous != null)
        {
            _ = DisconnectQuietlyAsync(previous);
        }
    }

    public void Clear()
    {
        Instance = null;
    }

    private static async Task DisconnectQuietlyAsync(TCPClient client)
    {
        try
        {
            await client.DisconnectAsync();
        }
        catch
        {
            // соединение уже мертво — не важно
        }
    }
}