using System;

public class CorrectAnswer
{
    public Guid Id { get; set; }

    public Guid QuestionId { get; set; }

    public Question Question { get; set; } = null!;

    public Guid? AnswerOptionId { get; set; }

    public AnswerOption? AnswerOption { get; set; }

    public string? Text { get; set; }
}