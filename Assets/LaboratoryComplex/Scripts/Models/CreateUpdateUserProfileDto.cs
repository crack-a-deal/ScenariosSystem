using System;

public class CreateUpdateUserProfileDto
{
    public Guid UserAccountId { get; }

    public string FirstName { get; }
    public string LastName { get; }
    public string? Patronymic { get; }

    public Guid UserGroupId { get; }

    public CreateUpdateUserProfileDto(
        Guid userAccountId,
        string firstName,
        string lastName,
        string? patronymic,
        Guid userGroupId)
    {
        UserAccountId = userAccountId;
        FirstName = firstName;
        LastName = lastName;
        Patronymic = patronymic;
        UserGroupId = userGroupId;
    }
}