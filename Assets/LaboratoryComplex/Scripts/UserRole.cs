using System;

public enum UserRole
{
    Teacher,
    Student,
}


public class UserSession
{
    public Guid AccountId { get; private set; }
    public Guid ProfileId { get; private set; }

    public UserRole? Role { get; private set; }
    public Guid? GroupId { get; private set; }

    public bool IsAuthenticated { get; private set; }

    public void SignIn(Guid accountId, Guid profileId, UserRole role, Guid? groupId = null)
    {
        if (accountId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(accountId));

        AccountId = accountId;
        ProfileId = profileId;
        Role = role;
        GroupId = groupId;
        IsAuthenticated = true;
    }

    public void SignOut()
    {
        AccountId = Guid.Empty;
        ProfileId = Guid.Empty;
        Role = null;
        GroupId = null;
        IsAuthenticated = false;
    }
}