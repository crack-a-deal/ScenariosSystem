public enum ScenarioMode
{
    Training = 0,
    Control = 1
}

public static class ScenarioModeExtensions
{
    public static string ToDisplayString(this ScenarioMode mode)
    {
        return mode switch
        {
            ScenarioMode.Training => "Обучение",
            ScenarioMode.Control => "Контроль",
            _ => mode.ToString()
        };
    }
}