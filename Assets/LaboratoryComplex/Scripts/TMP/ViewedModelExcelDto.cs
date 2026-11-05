
using MiniExcelLibs.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

public class ViewedModelExcelDto
{
    [ExcelColumnName("№")]
    public int Number { get; set; }

    [ExcelColumnName("Название модели")]
    public string ModelName { get; set; } = string.Empty;

    [ExcelColumnName("Дата и время просмотра")]
    public string ViewedAt { get; set; } = string.Empty;
}

public class ViewedModelsExportDTO
{
    public List<UserModelViewDto> Views { get; set; } = new();
}

public class ScenarioCompletionExcelDto
{
    [ExcelColumnName("№")]
    public int Number { get; set; }

    [ExcelColumnName("Сценарий")]
    public string ScenarioName { get; set; } = string.Empty;

    [ExcelColumnName("Дата прохождения")]
    public string CompletedAt { get; set; } = string.Empty;
}

public class ScenarioCompletionsExportDTO
{
    public List<UserScenarioCompletionDto> Completions { get; set; } = new();
}

public class ScenarioCompletionExcelMapper
{
    public List<ScenarioCompletionExcelDto> Map(
        IEnumerable<UserScenarioCompletionDto> completions)
    {
        return completions
            .Select((completion, index) => new ScenarioCompletionExcelDto
            {
                Number = index + 1,
                ScenarioName = completion.ScenarioName,
                CompletedAt = completion.CompletedAt.ToString("dd.MM.yyyy HH:mm:ss")
            })
            .ToList();
    }
}

public class ScenarioCompletionsExcelDefinition
    : IMultiSheetExcelExportDefinition
{
    private readonly ScenarioCompletionExcelMapper _mapper;

    public ScenarioCompletionsExcelDefinition(
        ScenarioCompletionExcelMapper mapper)
    {
        _mapper = mapper;
    }

    public Type DtoType =>
        typeof(ScenarioCompletionsExportDTO);

    public string FileName =>
        "ScenarioCompletions.xlsx";

    public ExcelWorkbookData Build(object dto)
    {
        var exportData =
            (ScenarioCompletionsExportDTO)dto;

        var workbook =
            new ExcelWorkbookData();

        var rows = _mapper.Map(
            exportData.Completions);

        workbook.Sheets.Add(
            "Пройденные сценарии",
            rows);

        return workbook;
    }
}