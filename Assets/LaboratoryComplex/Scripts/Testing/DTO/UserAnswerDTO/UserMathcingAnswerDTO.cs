using System.Collections.Generic;

public class UserMatchingAnswerDTO : UserBaseAnswerDTO
{
    public List<UserMatchingPairAnswerDTO> Pairs { get; set; }
        = new List<UserMatchingPairAnswerDTO>();
}