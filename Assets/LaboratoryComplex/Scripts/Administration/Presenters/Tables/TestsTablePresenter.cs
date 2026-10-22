using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
    private readonly TestSessionService _sessionService;
    private readonly TestEditorPanelPresenter _testEditor;
    private readonly StudentAssignmentPresenter _studentAssignment;

    private readonly Dictionary<TestRowView, TestDTO> _rows = new();

    public TestsTablePresenter(TestsTableView view, ITestService service, TestSessionService sessionService, TestEditorPanelPresenter testEditor, StudentAssignmentPresenter studentAssignment)
    {
        _view = view;
        _service = service;
        _sessionService = sessionService;
        _testEditor = testEditor;
        _studentAssignment = studentAssignment;

        _testEditor.Saved += _testEditor_Saved;
    }

    private async void _testEditor_Saved()
    {
        await Show();
    }

    public async Task Show()
    {
        _view.Clear();
        _rows.Clear();

        try
        {
            var tests = await _service.GetAllTestsAsync();

            Debug.Log($"{tests.Count} tests");

            for (int i = 0; i < tests.Count; i++)
            {
                TestDTO test = tests[i];
                AddRow(test,i);
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private void AddRow(TestDTO test, int index)
    {
        var row = _view.CreateRow();

        _rows[row] = test;

        row.SetIndex((index + 1).ToString("D2"));
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
            case TestRowButton.AssignStudent:
                _studentAssignment.Show(test);
                break;
            case TestRowButton.AssignAll:
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
