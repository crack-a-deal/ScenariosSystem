using Networking.Request;

public class AuthenticationRequest : IRequest<AuthenticationResponse>
{
    public string Login { get; set; }
    public string Password { get; set; }
}
