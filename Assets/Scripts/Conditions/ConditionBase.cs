[System.Serializable]
public abstract class ConditionBase
{
    public abstract bool Evaluate(ScenarioContext context, ScenarioEvent scenarioEvent);

    public virtual void Reset()
    {
    }
}
