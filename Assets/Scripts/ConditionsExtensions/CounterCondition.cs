using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CounterCondition : ConditionBase, IScenarioObjectsProvider
{
    [ListDrawerSettings(ShowFoldout = true)]
    [SerializeField]
    private List<ScenarioStateProvider> _objects = new();

    [SerializeField]
    private ScenarioObjectState _requiredState;

    public IReadOnlyList<ScenarioStateProvider> Objects => _objects;

    [MinValue(0)]
    [SerializeField]
    private int _requiredCount;

    public override bool Evaluate(ScenarioContext context, ScenarioEvent scenarioEvent)
    {
        var count = 0;

        foreach (var obj in _objects)
        {
            if (obj != null && obj.Is(_requiredState))
            {
                count++;
            }
        }

        return count >= _requiredCount;
    }

    [ShowInInspector]
    [ReadOnly]
    private int CurrentCount
    {
        get
        {
            var count = 0;

            foreach (var obj in _objects)
            {
                if (obj != null && obj.Is(_requiredState))
                {
                    count++;
                }
            }

            return count;
        }
    }
}
