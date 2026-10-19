using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MultipleChoiceQuestionEditorPresenter : IQuestionEditorPresenter
{
    public event Action CancelClicked;
    public event Action<BaseQuestionDTO> Saved;

    private readonly ChoiceQuestionEditorView _view;
    private readonly List<QuestionOptionModel> _options = new();
    private readonly HashSet<Guid> _correctOptionIds = new();

    private Guid _questionId;
    private int _order;
    private string _questionText = string.Empty;

    public BaseQuestionDTO Question => GetQuestion();

    public MultipleChoiceQuestionEditorPresenter(ChoiceQuestionEditorView view, MultipleChoiceQuestionDTO question)
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

    private void Load(MultipleChoiceQuestionDTO question)
    {
        _questionId = question.Id;
        _order = question.Order;
        _questionText = question.Text ?? string.Empty;

        _options.Clear();

        foreach (AnswerOptionDTO option in question.Options ?? new List<AnswerOptionDTO>())
            _options.Add(new QuestionOptionModel(option.Id, option.Text ?? string.Empty));

        _correctOptionIds.Clear();

        if (question.CorrectOptionIds != null)
        {
            foreach (Guid id in question.CorrectOptionIds)
            {
                if (_options.Any(x => x.Id == id))
                    _correctOptionIds.Add(id);
            }
        }
    }

    public void Show()
    {
        _view.SetQuestionText(_questionText);
        _view.ClearOptions();
        _view.SetSelectionMode(false);

        foreach (QuestionOptionModel option in _options)
            _view.AddOption(option.Id, option.Text, _correctOptionIds.Contains(option.Id));
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
        _correctOptionIds.Remove(optionId);

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
        if (FindOption(optionId) == null)
            return;

        if (value)
            _correctOptionIds.Add(optionId);
        else
            _correctOptionIds.Remove(optionId);
    }

    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(_questionText))
            return false;

        if (_options.Count < 2)
            return false;

        if (_options.Any(x => string.IsNullOrWhiteSpace(x.Text)))
            return false;

        return _correctOptionIds.Count > 0 &&
               _correctOptionIds.All(id => _options.Any(x => x.Id == id));
    }

    public BaseQuestionDTO GetQuestion()
    {
        return new MultipleChoiceQuestionDTO
        {
            Id = _questionId,
            Text = _questionText,
            Type = QuestionType.MultipleChoice,
            Order = _order,
            Options = _options.Select((x, index) => new AnswerOptionDTO
            {
                Id = x.Id,
                Text = x.Text,
                Order = index + 1
            }).ToList(),
            CorrectOptionIds = _correctOptionIds.ToList()
        };
    }

    private QuestionOptionModel FindOption(Guid id)
    {
        return _options.FirstOrDefault(x => x.Id == id);
    }

    private void OnSaveClicked()
    {
        if (Validate())
            Saved?.Invoke(GetQuestion());
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