using System;
using UnityEngine;

public class ScenarioRunner : MonoBehaviour
{
    [SerializeField] private Scenario _scenario;
    private ScenarioContext _context;
    [SerializeField] ScenarioEventBus _eventBus;

    private int _currentStepIndex = -1;

    public event Action<Step> StepStarted;
    public event Action<Step> StepCompleted;
    public event Action<ScenarioError> ErrorOccurred;
    public event Action ScenarioCompleted;


    public Step CurrentStep =>
        _currentStepIndex >= 0 && _currentStepIndex < _scenario.Steps.Count
            ? _scenario.Steps[_currentStepIndex]
            : null;

    private void Awake()
    {
        Debug.Log($"Awake scenario runner for scenario: {_scenario.Title}");
        _context = new ScenarioContext();
        _eventBus.EventPublished += ProcessEvent;
    }

    public void Start()
    {
        Debug.Log($"Starting scenario: {_scenario.Title}");
        _currentStepIndex = 0;

        if (_scenario.Steps.Count == 0)
        {
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

    public void Dispose()
    {
        _eventBus.EventPublished -= ProcessEvent;
    }
}
