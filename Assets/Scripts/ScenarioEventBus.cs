using System;
using UnityEngine;

public class ScenarioEventBus : MonoBehaviour
{
    public event Action<ScenarioEvent> EventPublished;

    public void Publish(ScenarioEvent scenarioEvent)
    {
        Debug.Log($"Publishing event: {scenarioEvent.Type} from source: {scenarioEvent.Source}");
        EventPublished?.Invoke(scenarioEvent);
    }
}