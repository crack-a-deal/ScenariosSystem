public class UserProfileExcelMapper
{
    public UserProfileExcelDto Map(UserProfileDto dto)
    {
        return new UserProfileExcelDto
        {
            Login = dto.Login,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Patronymic = dto.Patronymic,
            GroupNumber = dto.GroupNumber,
            GroupName = dto.GroupName
        };
    }
}