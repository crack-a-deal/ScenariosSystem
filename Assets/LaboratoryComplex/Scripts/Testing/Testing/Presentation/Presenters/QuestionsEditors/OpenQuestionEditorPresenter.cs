using System;

public class OpenQuestionEditorPresenter : IQuestionEditorPresenter
{
    public event Action CancelClicked;
    public event Action<BaseQuestionDTO> Saved;

    private readonly OpenQuestionEditorView _view;

    private Guid _questionId;
    private int _order;
    private string _questionText = string.Empty;
    private string _correctAnswer = string.Empty;

    public BaseQuestionDTO Question => GetQuestion();

    public OpenQuestionEditorPresenter(OpenQuestionEditorView view, OpenAnswerQuestionDTO question)
    {
        _view = view;

        Subscribe();
        Load(question);
    }

    private void Subscribe()
    {
        _view.QuestionTextChanged += OnQuestionTextChanged;
        _view.CorrectAnswerChanged += OnCorrectAnswerChanged;
        _view.SaveClicked += OnSaveClicked;
        _view.CancelClicked += OnCancelClicked;
    }

    private void Load(OpenAnswerQuestionDTO question)
    {
        _questionId = question.Id;
        _order = question.Order;
        _questionText = question.Text ?? string.Empty;
        _correctAnswer = question.CorrectAnswer ?? string.Empty;
    }

    public void Show()
    {
        _view.SetQuestionText(_questionText);
        _view.SetCorrectAnswer(_correctAnswer);
    }

    public void Hide()
    {
        _view.gameObject.SetActive(false);
    }
    private void OnQuestionTextChanged(string text)
    {
        _questionText = text;
    }

    private void OnCorrectAnswerChanged(string answer)
    {
        _correctAnswer = answer;
    }

    public bool Validate()
    {
        return !string.IsNullOrWhiteSpace(_questionText) &&
               !string.IsNullOrWhiteSpace(_correctAnswer);
    }

    public BaseQuestionDTO GetQuestion()
    {
        return new OpenAnswerQuestionDTO
        {
            Id = _questionId,
            Text = _questionText,
            Type = QuestionType.OpenAnswer,
            Order = _order,
            CorrectAnswer = _correctAnswer
        };
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
        _view.CorrectAnswerChanged -= OnCorrectAnswerChanged;
        _view.SaveClicked -= OnSaveClicked;
        _view.CancelClicked -= OnCancelClicked;
    }
}