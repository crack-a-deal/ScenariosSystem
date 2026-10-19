using System;

public class RegisterUserDto
{
    public string Login { get; }
    public string Password { get; }

    public string FirstName { get; }
    public string LastName { get; }
    public string? Patronymic { get; }

    public Guid UserGroupId { get; }

    public RegisterUserDto(
        string login,
        string password,
        string firstName,
        string lastName,
        string? patronymic,
        Guid userGroupId)
    {
        Login = login;
        Password = password;
        FirstName = firstName;
        LastName = lastName;
        Patronymic = patronymic;
        UserGroupId = userGroupId;
    }
}