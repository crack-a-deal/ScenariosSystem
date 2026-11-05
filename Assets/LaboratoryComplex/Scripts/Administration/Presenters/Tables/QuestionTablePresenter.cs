using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static QuestionRowView;

public class QuestionTablePresenter
{
    public event Action QuestionsChanged;

    private readonly QuestionTableView _view;
    private readonly TestEditorPresenter _editorPresenter;
    private readonly ITestService _testService;
    private List<BaseQuestionDTO> _questionsList = new List<BaseQuestionDTO>();
    public IReadOnlyList<BaseQuestionDTO> Questions => _questionsList;

    private List<BaseQuestionDTO> _availableQuestions = new List<BaseQuestionDTO>();

    private Guid _currentTestId;
    private Guid _originalQuestionId;
    private Guid _cloneQuestionId;

    private readonly Dictionary<QuestionRowView, BaseQuestionDTO> _rows = new();

    public QuestionTablePresenter(QuestionTableView view, TestEditorPresenter editorPresenter,ITestService testService)
    {
        _view = view;
        _editorPresenter = editorPresenter;
        _testService = testService;
        _view.AddQuestionClicked += OnAddQuestionClicked;
        _editorPresenter.QuestionSaved += OnQuestionSaved;
        _editorPresenter.Canceled += OnCanceled;
    }

    private void OnCanceled()
    {
        _editorPresenter.Hide();
        Show();
    }

    public void SetQuestions(List<BaseQuestionDTO> questionDTOs, Guid testId)
    {
        _questionsList = questionDTOs ?? new List<BaseQuestionDTO>();
        _currentTestId = testId;
    }


    public void Show()
    {
        _view.gameObject.SetActive(true);
        _view.Clear();
        _rows.Clear();

        foreach (BaseQuestionDTO question in _questionsList)
        {
            AddRow(question);
        }

        _ = LoadAvailableQuestionsAsync();
    }

    public void Hide()
    {
        _view.gameObject.SetActive(false);
    }

    private async Task LoadAvailableQuestionsAsync()
    {
        _view.ClearAvailableQuestions();

        try
        {
            _availableQuestions =
                await _testService.GetAvailableQuestionsAsync();

            _view.SetAvailableQuestions(_availableQuestions);
            _view.EnableQuestionsDropdown();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Ошибка загрузки вопросов: {ex.Message}");
            _view.SetAvailableQuestions(new List<BaseQuestionDTO>());
            _view.EnableQuestionsDropdown();
        }
    }

    private void AddRow(BaseQuestionDTO question)
    {
        var row = _view.CreateRow();

        _rows[row] = question;

        row.SetTitle(question.Text);
        row.SetIndex(question.Order.ToString("D2"));

        string questionType = string.Empty;
        switch (question.Type)
        {
            case QuestionType.SingleChoice:
                questionType = "Выбор одного правильного ответа из предложенных вариантов";
                break;
            case QuestionType.MultipleChoice:
                questionType = "Выбор нескольких правильных ответов из предложенных вариантов";
                break;
            case QuestionType.TrueFalse:
                questionType = "Определение истинности или ложности представленных утверждений";
                break;
            case QuestionType.OpenAnswer:
                questionType = "Опишите основные принципы поставленного вопроса";
                break;
            case QuestionType.Matching:
                questionType = "Соответсвие элементов в правильной последовательности";
                break;
            case QuestionType.Ordering:
                questionType = "Порядок элементов в правильной последовательности";
                break;
        }

        row.SetQuestioType(questionType);


        row.ButtonClicked +=
            button => HandleButton(row, button);
    }

    private void HandleButton(QuestionRowView row, QuestionRowButton button)
    {
        if (!_rows.TryGetValue(row, out var test))
            return;

        switch (button)
        {
            case QuestionRowButton.Edit:
                Edit(test);
                break;

            case QuestionRowButton.Delete:
                Delete(test);
                break;
        }
    }

    private async void Edit(BaseQuestionDTO question)
    {
        BaseQuestionDTO editable = await PrepareEditableQuestionAsync(question);

        Hide();

        _editorPresenter.Show();
        _editorPresenter.ShowCurrentQuestion(editable);
    }

    private async Task<BaseQuestionDTO> PrepareEditableQuestionAsync(BaseQuestionDTO question)
    {
        ResetCloneState();

        bool usedByOtherTests = false;

        try
        {
            usedByOtherTests = await _testService
                .IsQuestionUsedByOtherTestsAsync(question.Id, _currentTestId);
        }
        catch (Exception exception)
        {

            Debug.LogException(exception);
            usedByOtherTests = true;
        }

        if (!usedByOtherTests)
            return question;

        BaseQuestionDTO clone = QuestionDtoFactory.Clone(question);
        clone.Order = question.Order;

        _originalQuestionId = question.Id;
        _cloneQuestionId = clone.Id;

        return clone;
    }

    private void ResetCloneState()
    {
        _originalQuestionId = Guid.Empty;
        _cloneQuestionId = Guid.Empty;
    }

    private void Delete(BaseQuestionDTO question)
    {
        _questionsList.Remove(question);

        UpdateQuestionOrder();

        QuestionsChanged?.Invoke();
        Show();
    }

    private void UpdateQuestionOrder()
    {
        for (int i = 0; i < _questionsList.Count; i++)
            _questionsList[i].Order = i + 1;
    }

    private void OnAddQuestionClicked(QuestionType type)
    {
        int selectedIndex = _view.SelectedAvailableQuestionIndex;

        if (selectedIndex >= 0 &&
            selectedIndex < _availableQuestions.Count)
        {
            BaseQuestionDTO question = _availableQuestions[selectedIndex];

            if (_questionsList.Exists(q => q.Id == question.Id))
            {
                Debug.Log("Этот вопрос уже добавлен в тест.");
                _view.ResetAvailableQuestionsSelection();
                return;
            }

            question.Order = _questionsList.Count + 1;
            _questionsList.Add(question);

            _view.ResetAvailableQuestionsSelection();

            QuestionsChanged?.Invoke();
            Show();

            return;
        }

        BaseQuestionDTO newQuestion =
            QuestionDtoFactory.Create(type, _questionsList.Count + 1);

        Hide();
        _editorPresenter.Show();
        _editorPresenter.ShowCurrentQuestion(newQuestion);
    }

    private void OnQuestionSaved(BaseQuestionDTO question)
    {
        if (question.Id == _cloneQuestionId && _originalQuestionId != Guid.Empty)
        {
            int originalIndex = _questionsList.FindIndex(q => q.Id == _originalQuestionId);

            if (originalIndex >= 0)
                _questionsList[originalIndex] = question;
            else
                _questionsList.Add(question);
        }

        ResetCloneState();

        int index = _questionsList.FindIndex(q => q.Id == question.Id);

        if (index >= 0)
        {
            question.Order = _questionsList[index].Order;
            _questionsList[index] = question;
        }
        else
        {
            question.Order = _questionsList.Count + 1;
            _questionsList.Add(question);
        }

        QuestionsChanged?.Invoke();
        Show();
    }
}