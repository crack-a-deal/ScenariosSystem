using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class TestEditorPanelPresenter : IDisposable
{
    private readonly TestEditorPanelView _view;
    private readonly QuestionTablePresenter _questionTable;
    private readonly ITestService _testService;

    private TestDTO _currentTest;
    private bool _isSaving;

    public event Action Saved;
    public event Action<string> ErrorOccurred;
    public event Action<bool> SavingChanged;

    public TestEditorPanelPresenter(
        TestEditorPanelView view,
        QuestionTablePresenter questionTable,
        ITestService testService)
    {
        _view = view;
        _questionTable = questionTable;
        _testService = testService;

        _view.BackClicked += OnBackClicked;
        _view.SaveClicked += OnSaveClicked;
        _questionTable.QuestionsChanged += OnQuestionsChanged;
    }

    public void SetTest(TestDTO test)
    {
        if (test == null)
            throw new ArgumentNullException(nameof(test));

        _currentTest = test;
        _currentTest.Questions ??= new System.Collections.Generic.List<BaseQuestionDTO>();

        _view.SetTestName(_currentTest.Title);
        _questionTable.SetQuestions(_currentTest.Questions);
    }

    public void Show()
    {
        if (_currentTest == null)
        {
            ErrorOccurred?.Invoke("Сначала необходимо выбрать тест.");
            return;
        }

        _view.gameObject.SetActive(true);
        _questionTable.SetQuestions(_currentTest.Questions);
        _questionTable.Show();
    }

    public void Hide()
    {
        _questionTable.Hide();
        _view.gameObject.SetActive(false);
    }

    public async Task SaveAsync()
    {
        if (_isSaving || _currentTest == null)
            return;

        _currentTest.Title = _view.GetTestName();

        if (string.IsNullOrWhiteSpace(_currentTest.Title))
        {
            ErrorOccurred?.Invoke("Укажите название теста.");
            return;
        }

        if (_currentTest.Questions == null || _currentTest.Questions.Count == 0)
        {
            ErrorOccurred?.Invoke("Добавьте хотя бы один вопрос.");
            return;
        }

        _isSaving = true;
        SavingChanged?.Invoke(true);

        try
        {
            UpdateQuestionOrder();

            await _testService.SaveAsync(_currentTest);

            Debug.Log($"Тест «{_currentTest.Title}» успешно сохранён.");

            Saved?.Invoke();
            Hide();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            ErrorOccurred?.Invoke($"Ошибка сохранения теста: {exception.Message}");
        }
        finally
        {
            _isSaving = false;
            SavingChanged?.Invoke(false);
        }
    }

    private void OnQuestionsChanged()
    {
        if (_currentTest == null)
            return;

        UpdateQuestionOrder();
    }

    private void UpdateQuestionOrder()
    {
        for (int i = 0; i < _currentTest.Questions.Count; i++)
            _currentTest.Questions[i].Order = i + 1;
    }

    private void OnBackClicked()
    {
        Hide();
    }
    private async void OnSaveClicked()
    {
        await SaveAsync();
    }

    public void Dispose()
    {
        _view.BackClicked -= OnBackClicked;
        _view.SaveClicked -= OnSaveClicked;
        _questionTable.QuestionsChanged -= OnQuestionsChanged;
    }
}