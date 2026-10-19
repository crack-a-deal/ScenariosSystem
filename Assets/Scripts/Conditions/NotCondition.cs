using LBG;
using UnityEngine;

[System.Serializable]
public class NotCondition : ConditionBase
{
    [SerializeReference]
    [SubclassSelector]
    private ConditionBase _condition;

    public override bool Evaluate(ScenarioContext context, ScenarioEvent scenarioEvent)
    {
        return _condition != null && !_condition.Evaluate(context, scenarioEvent);
    }

    public override void Reset()
    {
        _condition?.Reset();
    }
}
