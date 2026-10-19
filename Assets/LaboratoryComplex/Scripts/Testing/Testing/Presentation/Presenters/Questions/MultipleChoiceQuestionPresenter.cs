using System;
using System.Collections.Generic;

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
        return new UserMultipleChoiceAnswerDTO
        {
            QuestionId = _question.Id,
            SelectedOptionIds = _selectedOptionIds
        };
    }
}
