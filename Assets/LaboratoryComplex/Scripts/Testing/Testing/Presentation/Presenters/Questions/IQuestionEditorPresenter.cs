using System;

public interface IQuestionEditorPresenter : IDisposable
{
    event Action CancelClicked;
    event Action<BaseQuestionDTO> Saved;

    BaseQuestionDTO Question { get; }

    void Show();
    void Hide();
}