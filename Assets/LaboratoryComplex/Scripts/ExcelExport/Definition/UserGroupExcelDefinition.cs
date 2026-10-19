using System;

public class UserGroupExcelDefinition : IExcelExportDefinition
{
    private readonly UserGroupExcelMapper _mapper;

    public UserGroupExcelDefinition(
        UserGroupExcelMapper mapper)
    {
        _mapper = mapper;
    }

    public Type DtoType =>
        typeof(UserGroupDto);

    public string FileName =>
        "UserGroups.xlsx";

    public object Map(object dto)
    {
        return _mapper.Map(
            (UserGroupDto)dto);
    }
}