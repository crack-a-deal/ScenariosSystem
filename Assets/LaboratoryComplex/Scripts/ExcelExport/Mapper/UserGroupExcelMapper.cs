public class UserGroupExcelMapper
{
    public UserGroupExcelDto Map(UserGroupDto dto)
    {
        return new UserGroupExcelDto
        {
            Number = dto.Number,
            Name = dto.Name,
            UserCount = dto.UserCount
        };
    }
}