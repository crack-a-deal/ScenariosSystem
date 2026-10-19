using MiniExcelLibs.Attributes;

public class AnswerOptionExcelDto
{
    [ExcelColumnName("№ вопроса")]
    public int QuestionNumber { get; set; }

    [ExcelColumnName("Вариант ответа")]
    public string Text { get; set; }

    [ExcelColumnName("Порядок")]
    public int Order { get; set; }

    [ExcelColumnName("Правильный")]
    public string IsCorrect { get; set; }
}