using System.Threading.Tasks;

public interface IApplicationState
{
    Task EnterAsync();
    Task ExitAsync();
}
