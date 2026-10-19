using MiniExcelLibs.Attributes;

public class UserGroupExcelDto
{
    [ExcelColumnName("№")]
    public int Number { get; set; }

    [ExcelColumnName("Название группы")]
    public string Name { get; set; }

    [ExcelColumnName("Количество пользователей")]
    public int UserCount { get; set; }
}