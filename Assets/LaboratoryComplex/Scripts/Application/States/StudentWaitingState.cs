using System.Threading.Tasks;

public class StudentWaitingState : BaseApplicationState
{
    private readonly StudentWaitingPresenter _presenter;
    public StudentWaitingState(IApplication application, StudentWaitingPresenter presenter) : base(application)
    {
        _presenter = presenter;
    }

    public override Task EnterAsync()
    {
        _presenter.AccessGranted += OnAccessGranted;
        _presenter.AccessDenied += OnAccessDenied;

        _presenter.Show();

        return Task.CompletedTask;
    }

    public override async Task ExitAsync()
    {
        _presenter.AccessGranted -= OnAccessGranted;
        _presenter.AccessDenied -= OnAccessDenied;

        await _presenter.HideAsync();
    }

    private async void OnAccessGranted()
    {
        await _application.ChangeStateAsync<StudentAuthorizationState>();
    }

    private void OnAccessDenied()
    {
        // Доступ пока не выдан.
        // Можно оставить пользователя на экране ожидания
        // или показать сообщение через Presenter.
    }
}