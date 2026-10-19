using System;

public class ReordableItemModel
{
    public Guid Id { get; set; }
    public string Text { get; set; }

    public ReordableItemModel(Guid id, string text)
    {
        Id = id;
        Text = text;
    }
}