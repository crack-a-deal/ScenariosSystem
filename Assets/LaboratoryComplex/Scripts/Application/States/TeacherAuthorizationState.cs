using System.Threading.Tasks;

public class TeacherAuthorizationState : BaseApplicationState
{
    private readonly AdminLoginPresenter _authorizationPresenter;

    public TeacherAuthorizationState(IApplication application, AdminLoginPresenter authorizationPresenter) : base(application)
    {
        _authorizationPresenter = authorizationPresenter;
    }

    public override Task EnterAsync()
    {
        _authorizationPresenter.AuthorizationSuccess += OnAuthorizationSuccess;
        _authorizationPresenter.BackClicked += OnBackClicked;
        _authorizationPresenter.Show();

        return Task.CompletedTask;
    }

    public override Task ExitAsync()
    {
        _authorizationPresenter.AuthorizationSuccess -= OnAuthorizationSuccess;
        _authorizationPresenter.BackClicked -= OnBackClicked;
        _authorizationPresenter.Hide();

        return Task.CompletedTask;
    }

    private async void OnAuthorizationSuccess()
    {
        await _application.ChangeStateAsync<TeacherMainState>();
    }

    private async void OnBackClicked()
    {
        await _application.ChangeStateAsync<RoleSelectionState>();
    }
}