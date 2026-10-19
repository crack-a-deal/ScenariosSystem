using System;

public class UserAuthorizationResult
{
    public bool IsAuthorized { get; set; }

    public Guid? UserProfileId { get; set; }
}