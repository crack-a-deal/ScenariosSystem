using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class BooleanStateCondition : ConditionBase
{
    [Required]
    [SerializeField]
    private Object _provider;

    [SerializeField]
    private bool _expectedValue = true;

    public override bool Evaluate(ScenarioContext context, ScenarioEvent scenarioEvent)
    {
        if (_provider is not IBooleanStateProvider stateProvider)
        {
            return false;
        }

        return stateProvider.Value == _expectedValue;
    }
}
