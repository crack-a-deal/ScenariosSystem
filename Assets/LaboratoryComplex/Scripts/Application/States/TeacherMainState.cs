using System.Threading.Tasks;

public class TeacherMainState : BaseApplicationState
{
    private readonly AdministrationPanelPresenter _presenter;

    public TeacherMainState(IApplication application, AdministrationPanelPresenter presenter) : base(application)
    {
        _presenter = presenter;
    }

    public override Task EnterAsync()
    {
        _presenter.BackClicked += OnBackClicked;

        return _presenter.ShowAsync();
    }

    public override Task ExitAsync()
    {
        _presenter.BackClicked -= OnBackClicked;

        return _presenter.HideAsync();
    }

    private void OnBackClicked()
    {
        _application.ChangeStateAsync<TeacherAuthorizationState>();
    }
}
