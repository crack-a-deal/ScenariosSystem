using LBG;
using UnityEngine;

[System.Serializable]
public class ErrorRule
{
    [SerializeField]
    private string _id;

    [TextArea]
    [SerializeField]
    private string _message;

    [SerializeField]
    private ErrorSeverity _severity;

    [SerializeReference, SubclassSelector]
    private ConditionBase _condition;

    public bool Evaluate(ScenarioContext context, ScenarioEvent scenarioEvent)
    {
        return _condition != null && _condition.Evaluate(context, scenarioEvent);
    }

    public ScenarioError CreateError()
    {
        return new ScenarioError(_id, _message, _severity);
    }
}
