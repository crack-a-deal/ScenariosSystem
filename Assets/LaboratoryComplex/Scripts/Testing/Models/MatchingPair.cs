using System;

public class MatchingPair
{
    public Guid Id { get; set; }

    public Guid QuestionId { get; set; }

    public Question Question { get; set; } = null!;

    public string LeftText { get; set; } = string.Empty;

    public string RightText { get; set; } = string.Empty;
}