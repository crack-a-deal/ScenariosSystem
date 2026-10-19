using System.Threading.Tasks;

public class StudentMainState : BaseApplicationState
{
    private readonly LearningSectionsPresenter _presenter;

    public StudentMainState(IApplication application, StudentLearningSectionsPresenter presenter) : base(application)
    {
        _presenter = presenter;
    }

    public override Task EnterAsync()
    {
        _presenter.Show();

        _presenter.VirtuaSectionClicked += OnVirtualModuleClicked;
        _presenter.InspectionSectionClicked += OnInteractionModuleClicked;
        _presenter.TestingSectionClicked += OnTestingModuleClicked;
        _presenter.BackClicked += _presenter_BackClicked;

        return Task.CompletedTask;
    }

    private void _presenter_BackClicked()
    {
        _application.ChangeStateAsync<StudentAuthorizationState>();
    }

    public override Task ExitAsync()
    {
        _presenter.Hide();

        _presenter.VirtuaSectionClicked -= OnVirtualModuleClicked;
        _presenter.InspectionSectionClicked -= OnInteractionModuleClicked;
        _presenter.TestingSectionClicked -= OnTestingModuleClicked;

        return Task.CompletedTask;
    }

    private void OnVirtualModuleClicked()
    {
        _application.ChangeStateAsync<ScenarioState>();
    }

    private void OnInteractionModuleClicked()
    {
        _application.ChangeStateAsync<InspectionModelState>();
    }

    private void OnTestingModuleClicked()
    {
        _application.ChangeStateAsync<TestState>();
    }
}
