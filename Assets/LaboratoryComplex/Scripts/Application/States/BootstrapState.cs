using System.Threading.Tasks;

public class BootstrapState : BaseApplicationState
{
    public BootstrapState(IApplication application) : base(application)
    {
    }

    public override Task EnterAsync()
    {
        return _application.ChangeStateAsync<RoleSelectionState>();
    }

    public override Task ExitAsync()
    {
        return Task.CompletedTask;
    }
}
