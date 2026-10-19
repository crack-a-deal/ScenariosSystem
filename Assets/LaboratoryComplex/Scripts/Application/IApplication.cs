using Networking;
using System.Threading.Tasks;

public interface IApplication
{
    Task ChangeStateAsync<TState>() where TState : IApplicationState;
}
