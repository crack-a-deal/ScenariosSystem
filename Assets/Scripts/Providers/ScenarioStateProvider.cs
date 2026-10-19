using Sirenix.OdinInspector;
using UnityEngine;

public class ScenarioStateProvider : MonoBehaviour
{
    [ReadOnly]
    [ShowInInspector]
    public ScenarioObjectState State => _state;

    [SerializeField]
    private ScenarioObjectState _state;

    [SerializeField]
    private ScenarioEventBus _eventBus;

    public void SetState(ScenarioObjectState state)
    {
        _state = state;

        _eventBus?.Publish(new ScenarioEvent(ScenarioEventType.StateChanged, this));
    }

    public bool Is(ScenarioObjectState state)
    {
        return _state == state;
    }
}
