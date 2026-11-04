public class OpenQuestionPresenter : IQuestionPresenter
{
    private readonly OpenQuestionView _view;
    private readonly OpenAnswerQuestionDTO _question;

    private string _inputAnswer = string.Empty;

    public BaseQuestionDTO Question => _question;

    public OpenQuestionPresenter(OpenQuestionView view, OpenAnswerQuestionDTO question)
    {
        _view = view;
        _question = question;
    }

    public void Show()
    {
        _view.SetQuestionText(_question.Text);

        _view.EndEdit += View_OnEndEdit;
    }

    public void Hide()
    {
        _view.EndEdit -= View_OnEndEdit;
    }

    private void View_OnEndEdit(string input)
    {
        _inputAnswer = input;
    }

    public UserBaseAnswerDTO GetAnswer()
    {
        return new UserOpenAnswerDTO
        {
            QuestionId = _question.Id,
            Text = _inputAnswer
        };
    }
}