using System;
public class UserMatchingPairAnswer
{
    public Guid Id { get; set; }

    public Guid UserAnswerId { get; set; }

    public UserAnswer UserAnswer { get; set; } = null!;

    public Guid LeftPairId { get; set; }

    public MatchingPair LeftPair { get; set; } = null!;

    public Guid RightPairId { get; set; }

    public MatchingPair RightPair { get; set; } = null!;
}