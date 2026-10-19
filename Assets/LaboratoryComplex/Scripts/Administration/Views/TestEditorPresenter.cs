using System;
using UnityEngine;

public class TestEditorPresenter
{
    public event Action<BaseQuestionDTO> QuestionSaved;
    public event Action Canceled;

    private readonly TestEditorView _view;
    private readonly QuestionEditViewContainer _viewContainer;
    private readonly QuestionEditPresenterFactory _factory;

    private IQuestionEditorPresenter _currentQuestionPresenter;

    public TestEditorPresenter(TestEditorView view, QuestionEditViewContainer viewContainer)
    {
        _view = view;
        _viewContainer = viewContainer;
        _factory = new QuestionEditPresenterFactory(_viewContainer);
        _view.BackClicked += OnBackClicked;
    }

    private void OnBackClicked()
    {
        Canceled?.Invoke();
    }

    public void Show()
    {
        _view.gameObject.SetActive(true);
    }

    public void Hide()
    {
        CloseCurrentQuestion();
        _view.gameObject.SetActive(false);
    }

    public void ShowCurrentQuestion(BaseQuestionDTO question)
    {
        CloseCurrentQuestion();

        _viewContainer.Show(question.Type);

        _currentQuestionPresenter = _factory.Create(question);
        _currentQuestionPresenter.CancelClicked += OnCancelClicked;
        _currentQuestionPresenter.Saved += OnQuestionSaved;

        _currentQuestionPresenter.Show();
    }

    private void OnQuestionSaved(BaseQuestionDTO question)
    {
        Hide();
        QuestionSaved?.Invoke(question);
    }

    private void OnCancelClicked()
    {
        CloseCurrentQuestion();
    }

    private void CloseCurrentQuestion()
    {
        if (_currentQuestionPresenter == null)
            return;

        _currentQuestionPresenter.Hide();

        _currentQuestionPresenter.CancelClicked -= OnCancelClicked;
        _currentQuestionPresenter.Saved -= OnQuestionSaved;

        _currentQuestionPresenter.Dispose();
        _currentQuestionPresenter = null;
    }
}