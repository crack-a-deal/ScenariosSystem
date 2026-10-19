using System.Collections.Generic;

public class JsonTestDTO
{
    public string Title { get; set; } = string.Empty;
    public List<JsonQuestionDTO> Questions { get; set; }
        = new List<JsonQuestionDTO>();
}

public class JsonQuestionDTO
{
    public string Type { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;

    public List<JsonOptionDTO> Options { get; set; }
        = new List<JsonOptionDTO>();

    public string CorrectAnswer { get; set; } = string.Empty;

    public List<JsonMatchingPairDTO> Pairs { get; set; }
        = new List<JsonMatchingPairDTO>();
}

public class JsonOptionDTO
{
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool Correct { get; set; }
}

public class JsonMatchingPairDTO
{
    public string LeftText { get; set; } = string.Empty;
    public string RightText { get; set; } = string.Empty;
}