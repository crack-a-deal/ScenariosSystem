using Networking.Request;
using System;

public class AuthenticationResponse : IResponse
{
    public Guid AccountId { get; set; }
    public Guid ProfileId { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
}
