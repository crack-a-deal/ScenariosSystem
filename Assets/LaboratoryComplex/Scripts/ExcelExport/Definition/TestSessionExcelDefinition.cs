using System;
using System.Linq;

public class TestSessionExcelDefinition
    : IMultiSheetExcelExportDefinition
{
    private readonly TestSessionResultExcelMapper _resultMapper;

    public TestSessionExcelDefinition(
        TestSessionResultExcelMapper resultMapper)
    {
        _resultMapper = resultMapper;
    }

    public Type DtoType =>
        typeof(TestSessionExportDTO);

    public string FileName =>
        "TestSession.xlsx";

    public ExcelWorkbookData Build(object dto)
    {
        var exportData =
            (TestSessionExportDTO)dto;

        var workbook =
            new ExcelWorkbookData();

        var results =
            exportData.Test.Questions
                .Select(question =>
                {
                    var answer =
                        exportData.Session.Answers
                            .FirstOrDefault(
                                a => a.QuestionId == question.Id);

                    return _resultMapper.Map(
                        question,
                        answer);
                })
                .ToList();

        int total = results.Count;
        int correct = results.Count(
            result => result.Result == "Верно");

        results.Add(new TestSessionResultExcelDto
        {
            QuestionNumber = total,
            QuestionText = "ИТОГО",
            QuestionType = string.Empty,
            UserAnswer = string.Empty,
            CorrectAnswer = string.Empty,
            Result = $"Верно: {correct} из {total}"
        });

        workbook.Sheets.Add(
            "Результаты",
            results);

        return workbook;
    }
}