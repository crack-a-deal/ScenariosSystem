using System;
using UnityEngine;

public class ScenarioRunner : MonoBehaviour
{
    [SerializeField] private Scenario _scenario;
    [SerializeField] ScenarioEventBus _eventBus;
    [SerializeField] private ScenarioMode _mode = ScenarioMode.Training;

    private ScenarioContext _context;
    private System.Diagnostics.Stopwatch _stopwatch;
    private int _currentStepIndex = -1;

    public event Action<Step> StepStarted;
    public event Action<Step> StepCompleted;
    public event Action<ScenarioError> ErrorOccurred;
    public event Action ScenarioCompleted;
    public event Action<ScenarioMode> ModeChanged;

    public string ScenarioTitle => _scenario != null ? _scenario.Title : null;

    public ScenarioMode Mode => _mode;

    public double ElapsedSeconds =>
        _stopwatch != null && _stopwatch.IsRunning
            ? _stopwatch.Elapsed.TotalSeconds
            : 0d;

    public Step CurrentStep =>
        _currentStepIndex >= 0 && _currentStepIndex < _scenario.Steps.Count
            ? _scenario.Steps[_currentStepIndex]
            : null;

    private void Awake()
    {
        _context = new ScenarioContext();
        _eventBus.EventPublished += ProcessEvent;
    }

    private void OnDestroy()
    {
        _eventBus.EventPublished -= ProcessEvent;
    }

    public void SelectScenario(Scenario scenario)
    {
        _scenario = scenario;
    }

    public void SetMode(ScenarioMode mode)
    {
        if (_mode == mode)
            return;

        _mode = mode;
        ModeChanged?.Invoke(_mode);
    }

    public void SetTrainingMode()
    {
        _mode = ScenarioMode.Training;
        ModeChanged?.Invoke(_mode);
    }

    public void SetExamMode()
    {
        _mode = ScenarioMode.Control;
        ModeChanged?.Invoke(_mode);
    }

    public void StartScenario()
    {
        Debug.Log($"Starting scenario: {_scenario.Title} (mode: {_mode})");
        _currentStepIndex = 0;
        _stopwatch = System.Diagnostics.Stopwatch.StartNew();

        if (_scenario.Steps.Count == 0)
        {
            _stopwatch.Stop();
            ScenarioCompleted?.Invoke();
            return;
        }

        StartCurrentStep();
    }

    private void ProcessEvent(ScenarioEvent scenarioEvent)
    {
        var step = CurrentStep;

        if (step == null)
        {
            return;
        }

        Debug.Log($"Current step: {step.name}, IsCompleted: {step.IsCompleted}");

        var wasCompleted = step.IsCompleted;

        step.ProcessEvent(_context, scenarioEvent, HandleError);

        if (!wasCompleted && step.IsCompleted)
        {
            CompleteCurrentStep();
        }
    }

    private void CompleteCurrentStep()
    {
        Debug.Log($"Completing step: {CurrentStep.name}");
        var completedStep = CurrentStep;

        StepCompleted?.Invoke(completedStep);

        _currentStepIndex++;

        if (_currentStepIndex >= _scenario.Steps.Count)
        {
            Debug.Log($"Scenario completed: {_scenario.Title}");
            _stopwatch?.Stop();
            Debug.Log($"TIIIME: {ElapsedSeconds}");
            ScenarioCompleted?.Invoke();
            return;
        }

        StartCurrentStep();
    }

    private void StartCurrentStep()
    {
        var step = CurrentStep;
        Debug.Log($"Starting step: {step.name}");
        step.Enter(_context);

        StepStarted?.Invoke(step);
    }

    private void HandleError(ScenarioError error)
    {
        ErrorOccurred?.Invoke(error);
    }
}