using System;

public class UserAccountExcelDefinition : IExcelExportDefinition
{
    private readonly UserAccountExcelMapper _mapper;

    public UserAccountExcelDefinition(
        UserAccountExcelMapper mapper)
    {
        _mapper = mapper;
    }

    public Type DtoType =>
        typeof(UserAccountDto);

    public string FileName =>
        "UserAccounts.xlsx";

    public object Map(object dto)
    {
        return _mapper.Map(
            (UserAccountDto)dto);
    }
}