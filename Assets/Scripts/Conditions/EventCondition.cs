using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class EventCondition : ConditionBase
{
    [EnumToggleButtons]
    [SerializeField]
    private ScenarioEventType _eventType;

    [SerializeField]
    private GameObject _source;

    public override bool Evaluate(ScenarioContext context, ScenarioEvent scenarioEvent)
    {
        if (scenarioEvent.Type != _eventType)
        {
            return false;
        }

        if (_source == null)
        {
            return true;
        }

        return scenarioEvent.Source == _source;
    }
}
