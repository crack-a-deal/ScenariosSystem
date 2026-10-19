public class MatchingPairExcelMapper
{
    public MatchingPairExcelDto Map(
        MatchingQuestionDTO question,
        MatchingPairDTO pair)
    {
        return new MatchingPairExcelDto
        {
            QuestionNumber = question.Order,
            LeftText = pair.LeftText,
            RightText = pair.RightText
        };
    }
}