using System.Threading.Tasks;

public abstract class BaseApplicationState : IApplicationState
{
    protected readonly IApplication _application;

    public BaseApplicationState(IApplication application)
    {
        _application = application;
    }

    public abstract Task EnterAsync();
    public abstract Task ExitAsync();
}
