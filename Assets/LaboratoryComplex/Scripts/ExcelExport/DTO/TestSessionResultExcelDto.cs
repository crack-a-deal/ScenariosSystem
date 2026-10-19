using MiniExcelLibs.Attributes;

public class TestSessionResultExcelDto
{
    [ExcelColumnName("№ вопроса")]
    public int QuestionNumber { get; set; }

    [ExcelColumnName("Вопрос")]
    public string QuestionText { get; set; }

    [ExcelColumnName("Тип")]
    public string QuestionType { get; set; }

    [ExcelColumnName("Ответ пользователя")]
    public string UserAnswer { get; set; }

    [ExcelColumnName("Правильный ответ")]
    public string CorrectAnswer { get; set; }

    [ExcelColumnName("Результат")]
    public string Result { get; set; }
}