using System;
using System.Collections.Generic;

public class MultipleChoiceQuestionDTO : BaseQuestionDTO
{
    public List<AnswerOptionDTO> Options { get; set; }
        = new List<AnswerOptionDTO>();

    public List<Guid> CorrectOptionIds { get; set; }
        = new List<Guid>();
}