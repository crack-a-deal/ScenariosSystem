using System;

public interface IExcelExportDefinition
{
    Type DtoType { get; }

    object Map(object dto);

    string FileName { get; }
}