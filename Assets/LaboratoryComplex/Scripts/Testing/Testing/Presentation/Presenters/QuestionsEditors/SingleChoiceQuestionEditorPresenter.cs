using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SingleChoiceQuestionEditorPresenter : IQuestionEditorPresenter
{
    public event Action CancelClicked;
    public event Action<BaseQuestionDTO> Saved;

    private readonly ChoiceQuestionEditorView _view;
    private readonly List<QuestionOptionModel> _options = new();

    private Guid _questionId;
    private int _order;
    private string _questionText = string.Empty;
    private Guid? _correctOptionId;

    public BaseQuestionDTO Question => GetQuestion();

    public SingleChoiceQuestionEditorPresenter(ChoiceQuestionEditorView view, SingleChoiceQuestionDTO question)
    {
        _view = view;

        Subscribe();
        Load(question);
    }

    private void Subscribe()
    {
        _view.QuestionTextChanged += OnQuestionTextChanged;
        _view.AddOptionClicked += OnAddOptionClicked;
        _view.RemoveOptionClicked += OnRemoveOptionClicked;
        _view.OptionTextChanged += OnOptionTextChanged;
        _view.OptionValueChanged += OnOptionValueChanged;
        _view.SaveClicked += OnSaveClicked;
        _view.CancelClicked += OnCancelClicked;
    }

    private void Load(SingleChoiceQuestionDTO question)
    {
        _questionId = question.Id;
        _order = question.Order;
        _questionText = question.Text ?? string.Empty;

        _options.Clear();

        foreach (AnswerOptionDTO option in question.Options ?? new List<AnswerOptionDTO>())
            _options.Add(new QuestionOptionModel(option.Id, option.Text ?? string.Empty));

        _correctOptionId = question.CorrectOptionId;

        if (!_options.Any(x => x.Id == _correctOptionId))
            _correctOptionId = null;
    }

    public void Show()
    {
        _view.SetQuestionText(_questionText);
        _view.ClearOptions();
        _view.SetSelectionMode(true);
        _view.ClearError();

        foreach (QuestionOptionModel option in _options)
            _view.AddOption(option.Id, option.Text, option.Id == _correctOptionId);

        _view.SetAddOptionInteractable(_options.Count < 4);
    }

    public void Hide()
    {
        _view.gameObject.SetActive(false);
    }

    private void OnQuestionTextChanged(string text)
    {
        _questionText = text;
    }

    private void OnAddOptionClicked()
    {
        if (_options.Count >= 4)
        {
            _view.ShowError("Максимум 4 варианта ответа.");
            return;
        }

        _options.Add(new QuestionOptionModel(Guid.NewGuid(), string.Empty));
        Show();
    }

    private void OnRemoveOptionClicked(Guid optionId)
    {
        QuestionOptionModel option = FindOption(optionId);

        if (option == null)
            return;

        _options.Remove(option);

        if (_correctOptionId == optionId)
            _correctOptionId = null;

        Show();
    }

    private void OnOptionTextChanged(Guid optionId, string text)
    {
        QuestionOptionModel option = FindOption(optionId);

        if (option != null)
            option.Text = text;
    }

    private void OnOptionValueChanged(Guid optionId, bool value)
    {
        if (!value || FindOption(optionId) == null)
            return;

        _correctOptionId = optionId;
    }

    public bool Validate()
    {
        return string.IsNullOrWhiteSpace(GetValidationError());
    }

    public BaseQuestionDTO GetQuestion()
    {
        return new SingleChoiceQuestionDTO
        {
            Id = _questionId,
            Text = _questionText,
            Type = QuestionType.SingleChoice,
            Order = _order,
            Options = _options.Select((x, index) => new AnswerOptionDTO
            {
                Id = x.Id,
                Text = x.Text,
                Order = index + 1
            }).ToList(),
            CorrectOptionId = _correctOptionId
        };
    }

    private QuestionOptionModel FindOption(Guid id)
    {
        return _options.FirstOrDefault(x => x.Id == id);
    }

    private void OnSaveClicked()
    {
        if (Validate())
        {
            _view.ClearError();
            Saved?.Invoke(GetQuestion());
        }
        else
        {
            _view.ShowError(GetValidationError());
        }
    }

    private string GetValidationError()
    {
        if (string.IsNullOrWhiteSpace(_questionText))
            return "Введите текст вопроса.";

        if (_options.Count < 2)
            return "Добавьте минимум два варианта ответа.";

        if (_options.Any(x => string.IsNullOrWhiteSpace(x.Text)))
            return "Заполните текст всех вариантов ответа.";

        if (!_correctOptionId.HasValue || _options.All(x => x.Id != _correctOptionId.Value))
            return "Отметьте правильный вариант ответа.";

        return string.Empty;
    }

    private void OnCancelClicked()
    {
        CancelClicked?.Invoke();
    }

    public void Dispose()
    {
        _view.QuestionTextChanged -= OnQuestionTextChanged;
        _view.AddOptionClicked -= OnAddOptionClicked;
        _view.RemoveOptionClicked -= OnRemoveOptionClicked;
        _view.OptionTextChanged -= OnOptionTextChanged;
        _view.OptionValueChanged -= OnOptionValueChanged;
        _view.SaveClicked -= OnSaveClicked;
        _view.CancelClicked -= OnCancelClicked;
    }
}