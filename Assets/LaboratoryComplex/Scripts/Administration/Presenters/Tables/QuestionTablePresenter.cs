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

    private List<BaseQuestionDTO> _availableQuestions =
    new List<BaseQuestionDTO>();

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
        Debug.Log("SHOW0");
        _editorPresenter.Hide();
        Show();
    }

    public void SetQuestions(List<BaseQuestionDTO> questionDTOs)
    {
        _questionsList = questionDTOs ?? new List<BaseQuestionDTO>();
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

    private void Edit(BaseQuestionDTO question)
    {
        Debug.Log($"Edit question: {question.Id}, {question.Text}");

        Hide();

        _editorPresenter.Show();
        _editorPresenter.ShowCurrentQuestion(question);
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