using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class StudentTestSessionPresenter
{
    public event Action<SubmitTestResponse> TestCompleted;
    public event Action ExitRequested;


    private readonly TestSessionView _view;
    private readonly QuestionViewContainer _viewContainer;
    private readonly UserSession _userSession;
    private readonly TcpClientProvider _clientProvider;
    private readonly QuestionPresenterFactory _factory;
    private readonly CancellationTokenSource _lifetimeCts = new();

    private readonly Dictionary<Guid, UserBaseAnswerDTO> _answers = new();

    private TestDTO _currentTest;
    private Guid _sessionId;
    private int _currentQuestionIndex;
    private IQuestionPresenter _currentQuestionPresenter;
    private bool _isSubmitting;

    public StudentTestSessionPresenter(
        TestSessionView view,
        QuestionViewContainer viewContainer,
        UserSession userSession,
        TcpClientProvider clientProvider)
    {
        _view = view;
        _viewContainer = viewContainer;
        _userSession = userSession;
        _clientProvider = clientProvider;
        _factory = new QuestionPresenterFactory(_viewContainer);

        _view.SubmitClicked += View_OnSubmitClicked;
        _view.BackClicked += View_BackClicked;
    }

    private void View_BackClicked()
    {
        if (_isSubmitting)
            return;

        _currentQuestionPresenter?.Hide();
        _currentQuestionPresenter = null;

        _view.gameObject.SetActive(false);
        ExitRequested?.Invoke();
    }

    public async Task StartAsync(TestDTO test)
    {
        _currentTest = test;
        _currentQuestionIndex = 0;
        _answers.Clear();

        try
        {
            var response = await _clientProvider.Instance
                .SendRequestAsync<StartTestSessionResponse>(
                    new StartTestSessionRequest
                    {
                        TestId = _currentTest.Id,
                        UserId = _userSession.ProfileId
                    },
                    _lifetimeCts.Token);

            if (!response.Success || response.SessionId == Guid.Empty)
            {
                Debug.LogError("Не удалось получить сессию теста.");
                ExitRequested?.Invoke(); // возврат к списку вместо пустого экрана
                return;
            }

            _sessionId = response.SessionId;

            _view.gameObject.SetActive(true);
            ShowCurrentQuestion();
        }
        catch (OperationCanceledException)
        {
            // Запрос отменён.
        }
        catch (Exception exception)
        {
            Debug.LogError($"Не удалось начать тест: {exception}");
            ExitRequested?.Invoke(); // и при исключении — тоже к списку
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

        _currentQuestionPresenter?.Hide();
        _currentQuestionPresenter = _factory.Create(question);
        _currentQuestionPresenter.Show();
    }

    private async void View_OnSubmitClicked()
    {
        if (_isSubmitting || _currentQuestionPresenter == null)
            return;

        UserBaseAnswerDTO answer = _currentQuestionPresenter.GetAnswer();
        _answers[answer.QuestionId] = answer;

        if (_currentQuestionIndex < _currentTest.Questions.Count - 1)
        {
            _currentQuestionIndex++;
            ShowCurrentQuestion();
            return;
        }

        _isSubmitting = true;

        try
        {
            var response = await _clientProvider.Instance
                .SendRequestAsync<SubmitTestResponse>(
                    new SubmitTestRequest
                    {
                        SessionId = _sessionId,
                        Answers = new List<UserBaseAnswerDTO>(_answers.Values)
                    },
                    _lifetimeCts.Token);

            if (!response.Success)
            {
                Debug.LogError($"Не удалось отправить тест: {response.ErrorMessage}");
                return;
            }

            Debug.Log("Test submitted successfully");

            _view.gameObject.SetActive(false);
            TestCompleted?.Invoke(response);
        }
        catch (OperationCanceledException)
        {
            // Запрос отменён.
        }
        catch (Exception exception)
        {
            Debug.LogError($"Ошибка отправки теста: {exception}");
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    public void Dispose()
    {
        _view.SubmitClicked -= View_OnSubmitClicked;
        _view.BackClicked -= View_BackClicked;

        _lifetimeCts.Cancel();
        _lifetimeCts.Dispose();
    }
}