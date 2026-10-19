using Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class MyApplication : IApplication
{
    private Dictionary<Type, IApplicationState> _states;

    private IApplicationState _currentState;

    public void RegisterStates(IEnumerable<IApplicationState> states)
    {
        _states = states.ToDictionary(x => x.GetType());
    }

    public async Task StartAsync()
    {
        Debug.Log("Starting application...");
        await ChangeStateAsync<BootstrapState>();
    }

    public async Task Exit()
    {
        Debug.Log("Exiting application...");
        await _currentState.ExitAsync();
    }

    public async Task ChangeStateAsync<TState>() where TState : IApplicationState
    {
        var stateType = typeof(TState);

        if (!_states.TryGetValue(stateType, out var nextState))
            throw new InvalidOperationException($"State {stateType.Name} is not registered.");

        if (_currentState == nextState)
        {
            return;
        }

        if (_currentState != null)
        {
            await _currentState.ExitAsync();
        }

        Debug.Log($"Changing state from {_currentState?.GetType().Name ?? "None"} to {nextState.GetType().Name}");
        _currentState = nextState;
        await _currentState.EnterAsync();
    }
}
