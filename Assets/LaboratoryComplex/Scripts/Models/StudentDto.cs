using System;

public class StudentDto
{
    public Guid Id { get; }
    public string Login { get; }
    public string GroupName { get; }
    public int Number { get; }

    public StudentDto(
        Guid id,
        string login,
        string groupName,
        int number)
    {
        Id = id;
        Login = login;
        GroupName = groupName;
        Number = number;
    }
}
