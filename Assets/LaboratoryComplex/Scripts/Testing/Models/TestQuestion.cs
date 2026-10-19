using System;

public class TestQuestion
{
    public Guid TestId { get; set; }

    public Test Test { get; set; } = null!;

    public Guid QuestionId { get; set; }

    public Question Question { get; set; } = null!;

    public int Order { get; set; }
}
