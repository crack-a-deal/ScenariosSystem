using MiniExcelLibs.Attributes;

public class UserProfileExcelDto
{
    [ExcelColumnName("Логин")]
    public string Login { get; set; }

    [ExcelColumnName("Имя")]
    public string FirstName { get; set; }

    [ExcelColumnName("Фамилия")]
    public string LastName { get; set; }

    [ExcelColumnName("Отчество")]
    public string Patronymic { get; set; }

    [ExcelColumnName("№ группы")]
    public int GroupNumber { get; set; }

    [ExcelColumnName("Группа")]
    public string GroupName { get; set; }
}