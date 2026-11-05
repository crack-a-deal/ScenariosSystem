
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
    [ExcelColumnName("Студент")]
    public string StudentName { get; set; }

    [ExcelColumnName("Сценарий")]
    public string ScenarioName { get; set; }

    [ExcelColumnName("Режим")]
    public string Mode { get; set; }

    [ExcelColumnName("Длительность")]
    public string Duration { get; set; }

    [ExcelColumnName("Дата завершения")]
    public DateTime CompletedAt { get; set; }
}

public class ScenarioCompletionsExportDTO
{
    public List<UserScenarioCompletionDto> Completions { get; set; }
        = new List<UserScenarioCompletionDto>();
}

public class ScenarioCompletionExcelMapper
{
    public ScenarioCompletionExcelDto Map(UserScenarioCompletionDto completion)
    {
        return new ScenarioCompletionExcelDto
        {
            StudentName = completion.StudentName?.Trim(),
            ScenarioName = completion.ScenarioName,
            Mode = completion.Mode.ToDisplayString(),
            Duration = FormatDuration(completion.DurationSeconds),
            CompletedAt = completion.CompletedAt
        };
    }

    private string FormatDuration(double seconds)
    {
        if (seconds <= 0)
            return "—";

        return TimeSpan
            .FromSeconds(seconds)
            .ToString(@"hh\:mm\:ss");
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

        var rows = exportData.Completions
            .Select(completion =>
                _mapper.Map(completion))
            .ToList();

        workbook.Sheets.Add(
            "Прохождение сценариев",
            rows);

        return workbook;
    }
}