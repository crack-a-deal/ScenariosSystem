using System;
using System.Collections.Generic;

public class UserMultipleChoiceAnswerDTO : UserBaseAnswerDTO
{
    public List<Guid> SelectedOptionIds { get; set; }
        = new List<Guid>();
}