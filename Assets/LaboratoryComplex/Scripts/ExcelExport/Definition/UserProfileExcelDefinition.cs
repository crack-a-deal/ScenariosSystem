using System;

public class UserProfileExcelDefinition : IExcelExportDefinition
{
    private readonly UserProfileExcelMapper _mapper;

    public UserProfileExcelDefinition(
        UserProfileExcelMapper mapper)
    {
        _mapper = mapper;
    }

    public Type DtoType =>
        typeof(UserProfileDto);

    public string FileName =>
        "UserProfiles.xlsx";

    public object Map(object dto)
    {
        return _mapper.Map(
            (UserProfileDto)dto);
    }
}