using System;
using System.Collections.Generic;
public class UserAnswer
{
    public Guid Id { get; set; }

    public Guid TestSessionId { get; set; }

    public TestSession TestSession { get; set; } = null!;

    public Guid QuestionId { get; set; }

    public Question Question { get; set; } = null!;

    public string? Text { get; set; }

    public ICollection<UserAnswerOption> Options { get; set; }
    = new List<UserAnswerOption>();

    public ICollection<UserMatchingPairAnswer> MatchingPairs { get; set; }
        = new List<UserMatchingPairAnswer>();
}