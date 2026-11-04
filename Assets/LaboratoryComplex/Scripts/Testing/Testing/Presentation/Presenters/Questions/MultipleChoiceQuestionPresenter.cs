using System;
using System.Collections.Generic;
using UnityEngine;

public class MultipleChoiceQuestionPresenter : IQuestionPresenter
{
    private readonly ChoiceQuestionView _view;
    private readonly MultipleChoiceQuestionDTO _question;

    private List<Guid> _selectedOptionIds = new();

    public BaseQuestionDTO Question => _question;

    public MultipleChoiceQuestionPresenter(ChoiceQuestionView view, MultipleChoiceQuestionDTO question)
    {
        _view = view;
        _question = question;
    }

    public void Show()
    {
        _view.SetQuestionText(_question.Text);
        _view.SetOptions(_question.Options);
        _view.OptionValueChanged += View_OnOptionSelected;
    }

    public void Hide()
    {
        _view.OptionValueChanged -= View_OnOptionSelected;
    }

    private void View_OnOptionSelected(Guid optionId, bool value)
    {
        if (value)
        {
            _selectedOptionIds.Add(optionId);
        }
        else
        {
            _selectedOptionIds.Remove(optionId);
        }
    }

    public UserBaseAnswerDTO GetAnswer()
    {
        Debug.Log($"MultipleChoiceQuestionPresenter {_question.Id}");
        foreach (var item in _selectedOptionIds)
        {
            Debug.Log($"[{item}]");
        }

        return new UserMultipleChoiceAnswerDTO
        {
            QuestionId = _question.Id,
            SelectedOptionIds = _selectedOptionIds
        };
    }
}
