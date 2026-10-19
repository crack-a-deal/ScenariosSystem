using System;
using System.Threading.Tasks;
using UnityEngine;

public class AdminTestSessionPresenter
{
    private readonly TestSessionView _view;
    private readonly QuestionViewContainer _viewContainer;
    private readonly UserSession _userSession;
    private readonly TestSessionService _testSessionService;
    private readonly QuestionPresenterFactory _factory;

    private TestDTO _currentTest;
    private Guid _sessionId;
    private int _currentQuestionIndex;
    private IQuestionPresenter _currentQuestionPresenter;
    private bool _isSubmitting;

    public AdminTestSessionPresenter(
        TestSessionView view,
        QuestionViewContainer viewContainer,
        UserSession userSession,
        TestSessionService testSessionService)
    {
        _view = view;
        _viewContainer = viewContainer;
        _userSession = userSession;
        _testSessionService = testSessionService;
        _factory = new QuestionPresenterFactory(_viewContainer);

        _view.SubmitClicked += View_OnSubmitClicked;
    }

    public async void StartAsync(TestDTO test)
    {
        _currentTest = test;
        _currentQuestionIndex = 0;

        try
        {
            var session = await _testSessionService.StartAsync(
                _currentTest.Id,
                _userSession.ProfileId);

            if (session == null)
            {
                Debug.LogError("Не удалось получить сессию теста.");
                return;
            }

            _sessionId = session.Id;

            _view.gameObject.SetActive(true);
            ShowCurrentQuestion();
        }
        catch (Exception exception)
        {
            Debug.LogError($"Не удалось начать тест: {exception}");
        }
    }

    private void ShowCurrentQuestion()
    {
        BaseQuestionDTO question = _currentTest.Questions[_currentQuestionIndex];

        _viewContainer.Show(question.Type);
        _view.SetQuestionIndex(
            $"ВОПРОС {(_currentQuestionIndex + 1):D2} / {_currentTest.Questions.Count:D2}");

        string questionType = question.Type switch
        {
            QuestionType.SingleChoice => "Выберите один вариант ответа",
            QuestionType.MultipleChoice => "Выберите все подходящие варианты ответа",
            QuestionType.TrueFalse => "Выберите один вариант ответа",
            QuestionType.OpenAnswer => "Опишите основные принципы поставленного вопроса",
            QuestionType.Matching => "Используйте стрелки для перемещения по информационным блокам",
            QuestionType.Ordering => "Используйте стрелки для перемещения по информационным блокам",
            _ => string.Empty
        };

        _view.SetQuestionTypeText(questionType);

        _currentQuestionPresenter = _factory.Create(question);
        _currentQuestionPresenter.Show();
    }

    private async void View_OnSubmitClicked()
    {
        if (_isSubmitting || _currentQuestionPresenter == null)
            return;

        _currentQuestionPresenter.GetAnswer();

        if (_currentQuestionIndex < _currentTest.Questions.Count - 1)
        {
            _currentQuestionIndex++;
            ShowCurrentQuestion();
            return;
        }

        _isSubmitting = true;

        try
        {
            // Здесь можно добавить сохранение ответов и завершение сессии,
            // если администратор должен сохранять результаты прохождения.
            Debug.Log($"Просмотр теста завершён. SessionId: {_sessionId}");
            _view.gameObject.SetActive(false);
        }
        catch (Exception exception)
        {
            Debug.LogError($"Ошибка завершения теста: {exception}");
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    public void Dispose()
    {
        _view.SubmitClicked -= View_OnSubmitClicked;
    }
}
