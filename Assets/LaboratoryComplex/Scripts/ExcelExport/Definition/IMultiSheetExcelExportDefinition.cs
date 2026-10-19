using System;

public interface IMultiSheetExcelExportDefinition
{
    Type DtoType { get; }

    ExcelWorkbookData Build(object dto);

    string FileName { get; }
}