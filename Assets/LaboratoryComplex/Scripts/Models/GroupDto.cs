using System;


public class GroupDto
{
    public Guid Id { get; }

    public string Name { get; }

    public int StudentCount { get; }

    public int Number { get; }

    public GroupDto(
        Guid id,
        string name,
        int studentCount,
        int number)
    {
        Id = id;
        Name = name;
        StudentCount = studentCount;
        Number = number;
    }
}
