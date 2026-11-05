using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrueFalseQuestionEditorPresenter : IQuestionEditorPresenter
{
    public event Action CancelClicked;
    public event Action<BaseQuestionDTO> Saved;

    private readonly ChoiceQuestionEditorView _view;

    private Guid _questionId;
    private int _order;
    private Guid _trueOptionId;
    private Guid _falseOptionId;
    private string _questionText = string.Empty;
    private Guid? _correctOptionId;

    public BaseQuestionDTO Question => GetQuestion();

    public TrueFalseQuestionEditorPresenter(ChoiceQuestionEditorView view, TrueFalseQuestionDTO question)
    {
        _view = view;

        Subscribe();
        Load(question);
    }

    private void Subscribe()
    {
        _view.QuestionTextChanged += OnQuestionTextChanged;
        _view.OptionValueChanged += OnOptionValueChanged;
        _view.SaveClicked += OnSaveClicked;
        _view.CancelClicked += OnCancelClicked;
    }

    private void Load(TrueFalseQuestionDTO question)
    {
        _questionId = question.Id;
        _order = question.Order;
        _questionText = question.Text ?? string.Empty;

        List<AnswerOptionDTO> options = question.Options ?? new List<AnswerOptionDTO>();

        AnswerOptionDTO trueOption = options.FirstOrDefault(x => x.Order == 1)
                                     ?? options.FirstOrDefault(x => x.Text == "Верно")
                                     ?? options.ElementAtOrDefault(0);

        AnswerOptionDTO falseOption = options.FirstOrDefault(x => x.Order == 2)
                                      ?? options.FirstOrDefault(x => x.Text == "Неверно")
                                      ?? options.FirstOrDefault(x => trueOption == null || x.Id != trueOption.Id);

        _trueOptionId = trueOption?.Id ?? Guid.NewGuid();
        _falseOptionId = falseOption?.Id ?? Guid.NewGuid();

        if (_trueOptionId == _falseOptionId)
            _falseOptionId = Guid.NewGuid();

        _correctOptionId = question.CorrectOptionId;

        if (_correctOptionId != _trueOptionId && _correctOptionId != _falseOptionId)
            _correctOptionId = null;
    }

    public void Show()
    {
        _view.SetQuestionText(_questionText);
        _view.ClearOptions();
        _view.SetSelectionMode(true);
        _view.AddOption(_trueOptionId, "Верно", _correctOptionId == _trueOptionId);
        _view.AddOption(_falseOptionId, "Неверно", _correctOptionId == _falseOptionId);
        _view.ClearError();
    }

    public void Hide()
    {
        _view.gameObject.SetActive(false);
    }

    private void OnQuestionTextChanged(string text)
    {
        _questionText = text;
    }

    private void OnOptionValueChanged(Guid optionId, bool value)
    {
        if (!value)
            return;

        if (optionId != _trueOptionId && optionId != _falseOptionId)
            return;

        _correctOptionId = optionId;
    }

    public bool Validate()
    {
        return string.IsNullOrWhiteSpace(GetValidationError());
    }

    public BaseQuestionDTO GetQuestion()
    {
        return new TrueFalseQuestionDTO
        {
            Id = _questionId,
            Text = _questionText,
            Type = QuestionType.TrueFalse,
            Order = _order,
            Options = new List<AnswerOptionDTO>
            {
                new AnswerOptionDTO
                {
                    Id = _trueOptionId,
                    Text = "Верно",
                    Order = 1
                },
                new AnswerOptionDTO
                {
                    Id = _falseOptionId,
                    Text = "Неверно",
                    Order = 2
                }
            },
            CorrectOptionId = _correctOptionId
        };
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

        if (_correctOptionId != _trueOptionId && _correctOptionId != _falseOptionId)
            return "Выберите правильный ответ: «Верно» или «Неверно».";

        return string.Empty;
    }

    private void OnCancelClicked()
    {
        CancelClicked?.Invoke();
    }

    public void Dispose()
    {
        _view.QuestionTextChanged -= OnQuestionTextChanged;
        _view.OptionValueChanged -= OnOptionValueChanged;
        _view.SaveClicked -= OnSaveClicked;
        _view.CancelClicked -= OnCancelClicked;
    }
}