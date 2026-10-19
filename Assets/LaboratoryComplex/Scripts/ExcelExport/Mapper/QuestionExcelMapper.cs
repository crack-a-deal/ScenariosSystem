public class QuestionExcelMapper
{
    public QuestionExcelDto Map(
        BaseQuestionDTO question)
    {
        return new QuestionExcelDto
        {
            Number = question.Order,
            Text = question.Text,
            Type = GetQuestionTypeName(question.Type)
        };
    }

    private string GetQuestionTypeName(
        QuestionType type)
    {
        return type switch
        {
            QuestionType.SingleChoice =>
                "Один вариант",

            QuestionType.MultipleChoice =>
                "Несколько вариантов",

            QuestionType.TrueFalse =>
                "Да / Нет",

            QuestionType.OpenAnswer =>
                "Открытый ответ",

            QuestionType.Ordering =>
                "Порядок",

            QuestionType.Matching =>
                "Сопоставление",

            _ =>
                type.ToString()
        };
    }
}