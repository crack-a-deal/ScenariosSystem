
using Sirenix.OdinInspector;
using UnityEngine;

public class ScenarioStateProvider : MonoBehaviour
{
    [ReadOnly, ShowInInspector]
    public ScenarioObjectState State => _state;

    [SerializeField]
    private ScenarioObjectState _state;

    [SerializeField]
    private ScenarioEventBus _eventBus;

    [SerializeField]
    private Outline _highlightVisual;

    private void Awake()
    {
        _highlightVisual = GetComponent<Outline>();
        SetHighlighted(false);
    }

    public void SetHighlighted(bool highlighted)
    {
        if (_highlightVisual != null)
            _highlightVisual.enabled = highlighted;
    }

    public void SetState(ScenarioObjectState state)
    {
        _state = state;
        _eventBus?.Publish(
            new ScenarioEvent(ScenarioEventType.StateChanged, this));
    }

    public bool Is(ScenarioObjectState state)
    {
        return _state == state;
    }
}
