using System;
using System.Threading.Tasks;
using UnityEngine;

public class TestState : BaseApplicationState
{
    private readonly StudentTestModulesTablePresenter _tablePresenter;
    private readonly StudentTestSessionPresenter _testSessionPresenter;
    private readonly TestResultsView _resultsView;

    public TestState(IApplication application, StudentTestModulesTablePresenter tablePresenter, StudentTestSessionPresenter testSessionPresenter,TestResultsView testResultsView) : base(application)
    {
        _tablePresenter = tablePresenter;
        _testSessionPresenter = testSessionPresenter;
        _resultsView = testResultsView;
    }

    public override Task EnterAsync()
    {
        _tablePresenter.BackClicked += TablePresenterOnBackClicked;
        _tablePresenter.TestSelected += OnTestSelected;

        _testSessionPresenter.TestCompleted += OnTestCompleted;
        _testSessionPresenter.ExitRequested += OnTestExitRequested;
        _resultsView.ContinueClicked += OnResultsContinueClicked;

        _tablePresenter.Show();

        return Task.CompletedTask;
    }

    private void OnTestExitRequested()
    {
        _tablePresenter.Show();
    }

    private void OnTestSelected(TestDTO test)
    {
        _tablePresenter.Hide();
        _testSessionPresenter.StartAsync(test);
    }

    private void OnTestCompleted(SubmitTestResponse response)
    {
        _resultsView.gameObject.SetActive(true);
        _resultsView.Show(
            $"Ваш результат: {response.CorrectAnswers} / {response.TotalQuestions}");
    }

    private async void OnResultsContinueClicked()
    {
        _resultsView.gameObject.SetActive(false);
        await _application.ChangeStateAsync<StudentMainState>();
    }

    public override Task ExitAsync()
    {
        _tablePresenter.Hide();

        _tablePresenter.BackClicked -= TablePresenterOnBackClicked;
        _tablePresenter.TestSelected -= OnTestSelected;

        _testSessionPresenter.TestCompleted -= OnTestCompleted;
        _testSessionPresenter.ExitRequested -= OnTestExitRequested;
        _resultsView.ContinueClicked -= OnResultsContinueClicked;

        return Task.CompletedTask;
    }

    private async void TablePresenterOnBackClicked()
    {
        await _application.ChangeStateAsync<StudentMainState>();
    }
}
