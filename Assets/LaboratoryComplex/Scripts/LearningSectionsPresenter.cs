using NUnit.Framework.Internal;
using System;
using UnityEngine;
using Zenject;

public abstract class LearningSectionsPresenter : IDisposable
{
    public event Action VirtuaSectionClicked;
    public event Action InspectionSectionClicked;
    public event Action TestingSectionClicked;
    public event Action BackClicked;
    protected readonly LearningSectionsView _view;

    public LearningSectionsPresenter(LearningSectionsView view)
    {
        _view = view;
        _view.InspectionSectionClicked += OnInspectionSectionClicked;
        _view.VirtuaSectionClicked += OnVirtuaSectionClicked;
        _view.TestingSectionClicked += OnTestingSectionClicked;
        _view.BackClicked += OnBackClicked;
    }

    private void OnBackClicked()
    {
        BackClicked?.Invoke();
    }

    public void Dispose()
    {
        _view.InspectionSectionClicked -= OnInspectionSectionClicked;
        _view.VirtuaSectionClicked -= OnVirtuaSectionClicked;
        _view.TestingSectionClicked -= OnTestingSectionClicked;
        _view.BackClicked -= OnBackClicked;
    }

    public void Show()
    {
        _view.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _view.gameObject.SetActive(false);
    }

    private void OnInspectionSectionClicked()
    {
        InspectionSectionClicked?.Invoke();
    }

    private void OnVirtuaSectionClicked()
    {
        VirtuaSectionClicked?.Invoke();
    }

    private void OnTestingSectionClicked()
    {
        TestingSectionClicked?.Invoke();
    }
}

public class StudentLearningSectionsPresenter : LearningSectionsPresenter
{
    public StudentLearningSectionsPresenter([Inject(Id = "Student")] LearningSectionsView view) : base(view)
    {
    }
}

public class TeacherLearningSectionsPresenter : LearningSectionsPresenter
{
    private readonly AdminTestModulesTablePresenter _testsTablePresenter;
    private readonly TestSessionService _sessionService;
    private readonly IExcelExportService _exportService;
    private readonly IThreeDModelService _threeDModelService;
    private readonly IScenarioService _scenarioService;
    private readonly AdminTestSessionPresenter _sessionPresenter;
    private UserProfileDto _currentUser;

    public TeacherLearningSectionsPresenter(
        [Inject(Id = "Admin")] LearningSectionsView view,
        AdminTestModulesTablePresenter testsTablePresenter,
        TestSessionService sessionService,
        IExcelExportService exportService,
        IThreeDModelService threeDModel,
        IScenarioService scenarioService) : base(view)
    {
        _testsTablePresenter = testsTablePresenter;
        _sessionService = sessionService;
        _exportService = exportService;
        _threeDModelService = threeDModel;
        _scenarioService = scenarioService;

        _view.TestingSectionClicked += OnTestingSectionClicked;
        _view.InspectionSectionClicked += InspectionSectionClicked;
        _view.BackClicked += BackClicked;

        _testsTablePresenter.TestSelected += OnTestSelected;
        _testsTablePresenter.BackClicked += _testsTablePresenter_BackClicked;

        VirtuaSectionClicked += OnVirtualSectionClicked;
    }

    private async void InspectionSectionClicked()
    {
        var views = await _threeDModelService.GetAllViewsAsync();

        var exportData = new ViewedModelsExportDTO
        {
            Views = views
        };

        await _exportService.ExportMultiSheetAsync(exportData);
        Debug.Log($"path {_exportService.GetExportPath()}");
        _view.ShowPopup($"Результаты сохранены в\n {_exportService.GetExportPath()}");
        Debug.Log($"Результаты моделей экспортированы.");
    }

    private async void OnVirtualSectionClicked()
    {
        var completions = await _scenarioService.GetAllCompletionsAsync();

        await _exportService.ExportMultiSheetAsync(
            new ScenarioCompletionsExportDTO
            {
                Completions = completions
            });
        _view.ShowPopup($"Результаты сохранены в\n {_exportService.GetExportPath()}");
        Debug.Log($"Прохождения сценариев экспортированы.");
    }

    private void _testsTablePresenter_BackClicked()
    {
        _testsTablePresenter.Hide();
    }

    private void BackClicked()
    {
        _view.gameObject.SetActive(false);
    }

    public void SetUser(UserProfileDto userProfile)
    {
        _currentUser = userProfile;
        _testsTablePresenter.SetUser(_currentUser);
    }

    private async void OnTestSelected(TestDTO test)
    {
        try
        {
            var session = await _sessionService.GetByTestAndUserAsync(
                test.Id,
                _currentUser.Id);

            if (session == null)
            {
                Debug.LogWarning("Сессия для выбранного теста и студента не найдена.");
                return;
            }

            // ФИКС: раньше здесь вызывался TestSessionTestDataFactory.Create(test),
            // и в Excel попадали сфабрикованные ответы. Теперь — реальные ответы
            // студента, сохранённые при сдаче теста.
            var sessionDto =
                UserAnswerExportMapper.ToSessionDto(session, test);

            var exportData =
                new TestSessionExportDTO
                {
                    Test = test,
                    Session = sessionDto
                };

            await _exportService.ExportMultiSheetAsync(exportData);
            Debug.Log($"path {_exportService.GetExportPath()}");
            _view.ShowPopup($"Результаты сохранены в\n {_exportService.GetExportPath()}");
            Debug.Log($"Результаты теста {test.Title} экспортированы.");
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed to export test session: {exception}");
        }
    }

    private void OnTestingSectionClicked()
    {
        _testsTablePresenter.Show();
    }
}