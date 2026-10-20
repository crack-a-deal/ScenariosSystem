using System;

public class ViewedModelsExcelDefinition
    : IMultiSheetExcelExportDefinition
{
    private readonly ViewedModelExcelMapper _mapper;

    public ViewedModelsExcelDefinition(
        ViewedModelExcelMapper mapper)
    {
        _mapper = mapper;
    }

    public Type DtoType => typeof(ViewedModelsExportDTO);

    public string FileName => "ViewedModels.xlsx";

    public ExcelWorkbookData Build(object dto)
    {
        var exportData = (ViewedModelsExportDTO)dto;

        var workbook = new ExcelWorkbookData();

        var rows = _mapper.Map(exportData.Views);

        workbook.Sheets.Add("Просмотренные модели", rows);

        return workbook;
    }
}