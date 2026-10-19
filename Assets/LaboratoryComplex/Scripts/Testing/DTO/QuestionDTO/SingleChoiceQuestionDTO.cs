using System;
using System.Collections.Generic;

public class SingleChoiceQuestionDTO : BaseQuestionDTO
{
    public List<AnswerOptionDTO> Options { get; set; }
        = new List<AnswerOptionDTO>();

    public Guid? CorrectOptionId { get; set; }
}