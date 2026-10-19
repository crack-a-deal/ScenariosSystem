using System;

public class UserProfileDto
{
    public Guid Id { get; }
    public Guid UserAccountId { get; }

    public string Login { get; }

    public string FirstName { get; }
    public string LastName { get; }
    public string? Patronymic { get; }

    public Guid UserGroupId { get; }
    public string GroupName { get; }
    public int GroupNumber { get; }

    public UserProfileDto(
        Guid id,
        Guid userAccountId,
        string login,
        string firstName,
        string lastName,
        string? patronymic,
        Guid userGroupId,
        string groupName,
        int groupNumber)
    {
        Id = id;
        UserAccountId = userAccountId;
        Login = login;
        FirstName = firstName;
        LastName = lastName;
        Patronymic = patronymic;
        UserGroupId = userGroupId;
        GroupName = groupName;
        GroupNumber = groupNumber;
    }
}