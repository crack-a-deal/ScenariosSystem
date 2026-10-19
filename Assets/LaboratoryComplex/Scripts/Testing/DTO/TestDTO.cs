using System;
using System.Collections.Generic;

public class TestDTO
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public List<BaseQuestionDTO> Questions { get; set; }
        = new List<BaseQuestionDTO>();
}