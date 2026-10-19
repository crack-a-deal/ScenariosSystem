using System;

public class QuestionOptionModel
{
    public Guid Id { get; }

    public string Text { get; set; }

    public QuestionOptionModel(Guid id, string text)
    {
        Id = id;
        Text = text;
    }
}