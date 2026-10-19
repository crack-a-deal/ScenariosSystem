using System.Collections.Generic;

public class OrderingQuestionDTO : BaseQuestionDTO
{
    public List<AnswerOptionDTO> Options { get; set; }
        = new List<AnswerOptionDTO>();
}