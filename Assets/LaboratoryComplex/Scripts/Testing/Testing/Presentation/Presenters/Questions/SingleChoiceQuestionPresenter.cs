using System;
using UnityEngine;

public class SingleChoiceQuestionPresenter : IQuestionPresenter
{
    private readonly ChoiceQuestionView _view;
    private readonly SingleChoiceQuestionDTO _question;

    private Guid? _selectedOptionId;

    public BaseQuestionDTO Question => _question;

    public SingleChoiceQuestionPresenter(ChoiceQuestionView view, SingleChoiceQuestionDTO question)
    {
        _view = view;
        _question = question;
    }

    public void Show()
    {
        _view.SetQuestionText(_question.Text);
        _view.SetOptions(_question.Options);
        _view.SetToggleGroup();
        _view.OptionValueChanged += View_OnOptionSelected;
    }

    public void Hide()
    {
        _view.OptionValueChanged -= View_OnOptionSelected;
    }

    private void View_OnOptionSelected(Guid optionId, bool value)
    {
        if (!value)
            return;

        _selectedOptionId = optionId;
    }

    public UserBaseAnswerDTO GetAnswer()
    {
        Debug.Log($"SingleChoiceQuestionPresenter {_question.Id} selectedId:[{_selectedOptionId}]");
        return new UserSingleChoiceAnswerDTO
        {
            QuestionId = _question.Id,
            SelectedOptionId = _selectedOptionId
        };
    }
}
