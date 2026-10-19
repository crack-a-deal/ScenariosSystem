using System;

public abstract class BaseQuestionDTO
{
    public Guid Id { get; set; }

    public string Text { get; set; } = string.Empty;

    public QuestionType Type { get; set; }

    public int Order { get; set; }
}