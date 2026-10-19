public readonly struct ScenarioEvent
{
    public ScenarioEventType Type { get; }
    public object Source { get; }
    public object Data { get; }

    public ScenarioEvent(ScenarioEventType type, object source = null, object data = null)
    {
        Type = type;
        Source = source;
        Data = data;
    }
}