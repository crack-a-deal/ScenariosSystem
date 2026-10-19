using System;
public class UserAnswer
{
    public Guid Id { get; set; }

    public Guid TestSessionId { get; set; }

    public TestSession TestSession { get; set; } = null!;

    public Guid QuestionId { get; set; }

    public Question Question { get; set; } = null!;

    public string? Text { get; set; }
}