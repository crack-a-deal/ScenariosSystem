using System.Threading.Tasks;

public class StudentAuthorizationState : BaseApplicationState
{
    private readonly StudentLoginPresenter _authorizationPresenter;

    public StudentAuthorizationState(IApplication application, StudentLoginPresenter authorizationPresenter) : base(application)
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
        await _application.ChangeStateAsync<StudentMainState>();
    }

    private async void OnBackClicked()
    {
        await _application.ChangeStateAsync<RoleSelectionState>();
    }
}
