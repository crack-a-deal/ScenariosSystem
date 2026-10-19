public interface IQuestionPresenter
{
    BaseQuestionDTO Question { get; }
    void Show();
    UserBaseAnswerDTO GetAnswer();
}
