using System;

public class ThreeDModelDto
{
    public Guid Id { get; }
    public string Name { get; }

    public ThreeDModelDto(
        Guid id,
        string name)
    {
        Id = id;
        Name = name;
    }
}