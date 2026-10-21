using System;

namespace Networking
{
    public class ConnectedClient
    {
        public Guid Id { get; }
        public Connection Connection { get; }

        public string IpAddress { get; set; }
        public string HostName { get; set; }

        public bool IsAuthorized { get; private set; }

        public void Authorize()
        {
            IsAuthorized = true;
        }

        public ConnectedClient(Guid id, Connection connection, string ipAddress, string hostName)
        {
            Id = id;
            Connection = connection;
            IpAddress = ipAddress;
            HostName = hostName;
        }

        public ConnectedClient(Guid id, Connection connection)
        {
            Id = id;
            Connection = connection;
        }
    }
}