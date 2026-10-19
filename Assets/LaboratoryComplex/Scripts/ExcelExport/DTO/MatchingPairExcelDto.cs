using MiniExcelLibs.Attributes;

public class MatchingPairExcelDto
{
    [ExcelColumnName("№ вопроса")]
    public int QuestionNumber { get; set; }

    [ExcelColumnName("Левая часть")]
    public string LeftText { get; set; }

    [ExcelColumnName("Правая часть")]
    public string RightText { get; set; }
}