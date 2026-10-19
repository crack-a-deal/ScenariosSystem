using System;

public class UserAccountDto
{
    public Guid Id { get; }
    public string Login { get; }

    public UserAccountDto(
        Guid id,
        string login)
    {
        Id = id;
        Login = login;
    }
}