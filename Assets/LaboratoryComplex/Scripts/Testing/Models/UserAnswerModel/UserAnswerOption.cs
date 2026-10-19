using System;
public class UserAnswerOption
{
    public Guid Id { get; set; }

    public Guid UserAnswerId { get; set; }

    public UserAnswer UserAnswer { get; set; } = null!;

    public Guid AnswerOptionId { get; set; }

    public AnswerOption AnswerOption { get; set; } = null!;

    public int? Order { get; set; }
}