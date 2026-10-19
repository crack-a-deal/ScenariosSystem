using System;
using System.Collections.Generic;

public class Test
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public ICollection<TestQuestion> TestQuestions { get; set; }
        = new List<TestQuestion>();
}
