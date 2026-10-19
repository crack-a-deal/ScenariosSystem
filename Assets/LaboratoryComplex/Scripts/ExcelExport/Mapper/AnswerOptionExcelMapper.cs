using System.Linq;

public class AnswerOptionExcelMapper
{
    public AnswerOptionExcelDto Map(
        BaseQuestionDTO question,
        AnswerOptionDTO option)
    {
        bool isCorrect = false;

        if (question is SingleChoiceQuestionDTO singleChoice)
        {
            isCorrect =
                singleChoice.CorrectOptionId == option.Id;
        }
        else if (question is MultipleChoiceQuestionDTO multipleChoice)
        {
            isCorrect =
                multipleChoice.CorrectOptionIds
                    .Contains(option.Id);
        }
        else if (question is TrueFalseQuestionDTO trueFalse)
        {
            isCorrect =
                trueFalse.CorrectOptionId == option.Id;
        }

        return new AnswerOptionExcelDto
        {
            QuestionNumber = question.Order,
            Text = option.Text,
            Order = option.Order,
            IsCorrect = isCorrect ? "Да" : "Нет"
        };
    }
}