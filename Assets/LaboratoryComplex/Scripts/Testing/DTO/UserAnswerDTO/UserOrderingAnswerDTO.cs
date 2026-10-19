using System.Collections.Generic;

public class UserOrderingAnswerDTO : UserBaseAnswerDTO
{
    public List<UserOrderingItemDTO> Items { get; set; }
        = new List<UserOrderingItemDTO>();
}