public interface IQuestionPresenter
{
    BaseQuestionDTO Question { get; }
    void Show();
    void Hide();
    UserBaseAnswerDTO GetAnswer();
}
