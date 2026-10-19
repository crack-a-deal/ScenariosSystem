using System;

public class MatchingOptionModel
{
    public Guid Id { get; }

    public string LeftText { get; set; }

    public string RightText { get; set; }

    public MatchingOptionModel(
        Guid id,
        string leftText,
        string rightText)
    {
        Id = id;
        LeftText = leftText;
        RightText = rightText;
    }
}