using System;
using System.Collections.Generic;

public class ScenarioContext
{
    private readonly Dictionary<string, object> _values = new();

    public int ErrorCount { get; private set; }

    public void Set<T>(string key, T value)
    {
        _values[key] = value;
    }

    public bool TryGet<T>(string key, out T value)
    {
        if (_values.TryGetValue(key, out var rawValue) && rawValue is T typedValue)
        {
            value = typedValue;
            return true;
        }

        value = default;
        return false;
    }

    public T Get<T>(string key)
    {
        if (!_values.TryGetValue(key, out var value))
        {
            throw new InvalidOperationException($"Scenario value '{key}' was not found.");
        }

        return (T)value;
    }

    public void RegisterError()
    {
        ErrorCount++;
    }
}