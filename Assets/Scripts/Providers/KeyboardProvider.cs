using UnityEngine;

public class KeyboardProvider : MonoBehaviour
{
    [SerializeField]
    private KeyCode _code;

    [SerializeField]
    private ScenarioEventBus _eventBus;

    private void Update()
    {
        if (Input.GetKeyDown(_code))
        {
            SetState();
        }
    }

    public void SetState()
    {
        _eventBus?.Publish(new ScenarioEvent(ScenarioEventType.KeyPressed, gameObject));
    }
}
