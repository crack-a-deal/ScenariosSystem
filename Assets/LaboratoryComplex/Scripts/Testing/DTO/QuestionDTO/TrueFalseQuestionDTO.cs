using System;
using System.Collections.Generic;

public class TrueFalseQuestionDTO : BaseQuestionDTO
{
    public List<AnswerOptionDTO> Options { get; set; }
        = new List<AnswerOptionDTO>();

    public Guid? CorrectOptionId { get; set; }
}