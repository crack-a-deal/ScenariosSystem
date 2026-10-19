using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using static TestRowView;

public class TestsTablePresenter
{
    private readonly TestsTableView _view;
    private readonly ITestService _service;
    private readonly JsonTestImportService _testImportService;
    private readonly RandomTestGenerator _testGenerator;
    private readonly TestSessionService _sessionService;
    private readonly TestEditorPanelPresenter _testEditor;

    private readonly Dictionary<TestRowView, TestDTO> _rows = new();

    public TestsTablePresenter(TestsTableView view, ITestService service, JsonTestImportService testImportService,RandomTestGenerator testGenerator,TestSessionService sessionService, TestEditorPanelPresenter testEditor)
    {
        _view = view;
        _service = service;
        _testImportService = testImportService;
        _testGenerator = testGenerator;
        _sessionService = sessionService;
        _testEditor = testEditor;
    }

    public async Task Show()
    {
        _view.Clear();
        _rows.Clear();

        TestsConfig config = TestsConfigLoader.Load();

        string path = Path.Combine(
            Application.streamingAssetsPath,
            config.testBankFile);

        if (!File.Exists(path))
            throw new FileNotFoundException($"Test bank not found: {path}");

        try
        {
            bool imported = await _testImportService.ImportAsync(path);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }

        try
        {
            //Test test1 = await _testGenerator.CreateAsync("new random2");

            var tests = await _service.GetAllTestsAsync();

            Debug.Log($"{tests.Count} tests");

            foreach (var test in tests)
            {
                AddRow(test);
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private void AddRow(TestDTO test)
    {
        var row = _view.CreateRow();

        _rows[row] = test;

        row.SetTitle(test.Title);
        row.SetQuestioCount($"{test.Questions.Count} вопросов");

        row.ButtonClicked +=
            button => HandleButton(row, button);
    }

    private void HandleButton(
        TestRowView row,
        TestRowButton button)
    {
        if (!_rows.TryGetValue(row, out var test))
            return;

        switch (button)
        {
            case TestRowButton.Assign:
                Assing(test);
                break;
            case TestRowButton.Edit:
                Edit(test);
                break;

            case TestRowButton.Delete:
                Delete(test);
                break;
        }
    }

    private async void Assing(TestDTO test)
    {
        Debug.Log(
            $"assing test: {test.Id}, {test.Title}");
        await _sessionService.AssignToAllStudentsAsync(test.Id);
    }

    private void Edit(TestDTO test)
    {
        Debug.Log(
            $"Edit test: {test.Id}, {test.Title}");

        _testEditor.SetTest(test);
        _testEditor.Show();
    }

    private async void Delete(TestDTO test)
    {
        Debug.Log(
            $"Delete test: {test.Id}, {test.Title}");

        await _service.DeleteTestAsync(test.Id);

        await Show();
    }
}
