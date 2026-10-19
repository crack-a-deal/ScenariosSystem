using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class StudentTestModulesTablePresenter : IDisposable
{
    public event Action BackClicked;
    public event Action<TestDTO> TestSelected;

    private readonly ModulesTableView _view;
    private readonly TcpClientProvider _clientProvider;
    private readonly UserSession _userSession;
    private readonly Dictionary<ModuleItemView, TestListItemDTO> _rows = new();
    private readonly CancellationTokenSource _lifetimeCts = new();

    public StudentTestModulesTablePresenter(
        [Inject(Id = "StudentTestModule")] ModulesTableView view,
        TcpClientProvider clientProvider,
        UserSession userSession)
    {
        _view = view;
        _clientProvider = clientProvider;
        _userSession = userSession;

        _view.BackClicked += OnBackClicked;
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

        _lifetimeCts.Cancel();
        _lifetimeCts.Dispose();
    }

    private async Task LoadTestsAsync()
    {
        try
        {
            GetAvailableTestsResponse response =
                await _clientProvider.Instance.SendRequestAsync<GetAvailableTestsResponse>(
                    new GetAvailableTestsRequest
                    {
                        UserId = _userSession.ProfileId
                    },
                    _lifetimeCts.Token);

            Debug.Log(response.Tests.Count);
            if (response.Tests.Count == 0)
            {
                _view.ActiveteMessage(true);
                _view.SetMessage("Вам не назначено тестов");
                return;
            }
            _view.Clear();
            for (int i = 0; i < response.Tests.Count; i++)
            {
                AddRow(i, response.Tests[i]);
            }
        }
        catch (OperationCanceledException)
        {
            // Presenter is being disposed.
        }
        catch (Exception exception)
        {
            Debug.LogError($"[TEST CATALOG] Failed to load tests: {exception}");
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
            GetTestResponse response =
                await _clientProvider.Instance.SendRequestAsync<GetTestResponse>(
                    new GetTestRequest
                    {
                        TestId = testId
                    },
                    _lifetimeCts.Token);

            if (response.Test == null)
                Debug.LogWarning("Не удалось загрузить выбранный тест.");

            return response.Test;
        }
        catch (OperationCanceledException)
        {
            // Presenter is being disposed.
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
