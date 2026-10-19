public readonly struct ScenarioError
{
    public string Id { get; }
    public string Message { get; }
    public ErrorSeverity Severity { get; }

    public ScenarioError(string id, string message, ErrorSeverity severity)
    {
        Id = id;
        Message = message;
        Severity = severity;
    }
}
