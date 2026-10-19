using System.Collections.Generic;

public class MatchingQuestionDTO : BaseQuestionDTO
{
    public List<MatchingPairDTO> Pairs { get; set; }
        = new List<MatchingPairDTO>();
}