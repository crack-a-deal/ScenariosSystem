using System;

public class TrueFalseQuestionPresenter : IQuestionPresenter
{
    private readonly ChoiceQuestionView _view;
    private readonly TrueFalseQuestionDTO _question;

    private Guid? _selectedOptionId;

    public BaseQuestionDTO Question => _question;

    public TrueFalseQuestionPresenter(ChoiceQuestionView view, TrueFalseQuestionDTO question)
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

    private void View_OnOptionSelected(Guid optionId, bool value)
    {
        if (!value)
            return;

        _selectedOptionId = optionId;
    }

    public UserBaseAnswerDTO GetAnswer()
    {
        return new UserTrueFalseAnswerDTO
        {
            QuestionId = _question.Id,
            SelectedOptionId = _selectedOptionId
        };
    }
}