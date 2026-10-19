using MiniExcelLibs.Attributes;

public class UserAccountExcelDto
{
    [ExcelColumnName("Логин")]
    public string Login { get; set; }
}