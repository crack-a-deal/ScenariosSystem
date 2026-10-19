using MiniExcelLibs.Attributes;

public class TestExcelDto
{
    [ExcelColumnName("Название теста")]
    public string Title { get; set; }
}