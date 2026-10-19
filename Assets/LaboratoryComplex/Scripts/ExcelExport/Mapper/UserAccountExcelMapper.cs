public class UserAccountExcelMapper
{
    public UserAccountExcelDto Map(UserAccountDto dto)
    {
        return new UserAccountExcelDto
        {
            Login = dto.Login
        };
    }
}