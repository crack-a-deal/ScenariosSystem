using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class AdminTestModulesTablePresenter : IDisposable
{
    public event Action BackClicked;
    public event Action<TestDTO> TestSelected;

    private readonly ModulesTableView _view;
    private readonly ITestService _testService;
    private readonly TestSessionService _sessionService;
    private readonly Dictionary<ModuleItemView, TestListItemDTO> _rows = new();

    private UserProfileDto _currentUser;

    public AdminTestModulesTablePresenter(
        [Inject(Id = "AdminTestModule")] ModulesTableView view,
        ITestService testService,
        TestSessionService sessionService)
    {
        _view = view;
        _testService = testService;
        _sessionService = sessionService;

        _view.BackClicked += OnBackClicked;
    }

    public void SetUser(UserProfileDto student)
    {
        _currentUser = student;
    }

    public void Show()
    {
        _view.gameObject.SetActive(true);
        _view.Clear();
        _rows.Clear();
        _view.ActiveteMessage(false);

        _ = LoadTestsAsync();
    }

    public void Hide()
    {
        _view.gameObject.SetActive(false);
    }

    public void Dispose()
    {
        _view.BackClicked -= OnBackClicked;
    }

    private async Task LoadTestsAsync()
    {
        if (_currentUser == null)
        {
            Debug.LogWarning("Не выбран студент для просмотра тестов.");
            return;
        }

        try
        {
            var sessions =
                await _sessionService.GetByUserProfileIdAsync(_currentUser.Id);

            if (sessions.Count == 0)
            {
                _view.ActiveteMessage(true);
                _view.SetMessage("Вам не назначено тестов");
                return;
            }

            for (int i = 0; i < sessions.Count; i++)
            {
                var session = sessions[i];
                var test = session.Test;

                var item = new TestListItemDTO
                {
                    Id = test.Id,
                    Title = test.Title,
                    QuestionCount = test.TestQuestions.Count
                };

                AddRow(i, item);
            }
        }
        catch (OperationCanceledException)
        {
            // Обработка отмены, если позднее добавишь CancellationToken.
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed to load assigned tests: {exception}");
        }
    }

    private void AddRow(int index, TestListItemDTO itemDTO)
    {
        ModuleItemView row = _view.CreateRow();

        _rows[row] = itemDTO;

        row.SetTitle(itemDTO.Title);
        row.SetIndex((index + 1).ToString("D2"));
        row.ItemClicked += _ => HandleButton(row);
    }

    private async void HandleButton(ModuleItemView row)
    {
        if (!_rows.TryGetValue(row, out TestListItemDTO item))
            return;

        TestDTO test = await GetTestAsync(item.Id);

        if (test != null)
            TestSelected?.Invoke(test);
    }

    private async Task<TestDTO> GetTestAsync(Guid testId)
    {
        try
        {
            TestDTO test = await _testService.GetTestByIdAsync(testId);

            if (test == null)
                Debug.LogWarning("Не удалось загрузить выбранный тест.");

            return test;
        }
        catch (OperationCanceledException)
        {
            // Обработка отмены, если позднее добавишь CancellationToken.
        }
        catch (Exception exception)
        {
            Debug.LogError($"[TEST CATALOG] Failed to open test: {exception}");
        }

        return null;
    }

    private void OnBackClicked()
    {
        BackClicked?.Invoke();
    }
}