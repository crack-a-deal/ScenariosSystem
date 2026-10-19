using System;

public class AnswerOption
{
    public Guid Id { get; set; }

    public Guid QuestionId { get; set; }

    public Question Question { get; set; } = null!;

    public string Text { get; set; } = string.Empty;

    public int Order { get; set; }
}