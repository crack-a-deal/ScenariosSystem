using LBG;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AllCondition : ConditionBase
{
    [ListDrawerSettings(ShowFoldout = true)]
    [SerializeReference]
    [SubclassSelector]
    private List<ConditionBase> _conditions = new();

    public override bool Evaluate(ScenarioContext context, ScenarioEvent scenarioEvent)
    {
        foreach (var condition in _conditions)
        {
            if (!condition.Evaluate(context, scenarioEvent))
            {
                return false;
            }
        }

        return true;
    }

    public override void Reset()
    {
        foreach (var condition in _conditions)
        {
            condition.Reset();
        }
    }
}
