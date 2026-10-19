using MiniExcelLibs.Attributes;

public class QuestionExcelDto
{
    [ExcelColumnName("№")]
    public int Number { get; set; }

    [ExcelColumnName("Вопрос")]
    public string Text { get; set; }

    [ExcelColumnName("Тип")]
    public string Type { get; set; }
}