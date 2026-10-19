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
    private readonly AdminTestSessionPresenter _sessionPresenter;
    private UserProfileDto _currentUser;

    public TeacherLearningSectionsPresenter([Inject(Id = "Admin")] LearningSectionsView view, AdminTestModulesTablePresenter testsTablePresenter,TestSessionService sessionService ,IExcelExportService exportService) : base(view)
    {
        _testsTablePresenter = testsTablePresenter;
        _sessionService = sessionService;
        _exportService = exportService;

        _view.TestingSectionClicked += OnTestingSectionClicked;
        _view.BackClicked += BackClicked;
        _testsTablePresenter.TestSelected += OnTestSelected;
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

            var sessionDTO =TestSessionTestDataFactory.Create(test);

            var exportData =
                new TestSessionExportDTO
                {
                    Test = test,
                    Session = sessionDTO
                };

            await _exportService.ExportMultiSheetAsync(exportData);


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
