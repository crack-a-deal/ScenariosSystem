using System;

public class UserGroupDto
{
    public Guid Id { get; }
    public string Name { get; }
    public int Number { get; }
    public int UserCount { get; }

    public UserGroupDto(
        Guid id,
        string name,
        int number,
        int userCount)
    {
        Id = id;
        Name = name;
        Number = number;
        UserCount = userCount;
    }
}