using Networking.Request;

public class AccessRequest : IRequest<AccessResponse>
{
    public string HostName { get; set; }
}
