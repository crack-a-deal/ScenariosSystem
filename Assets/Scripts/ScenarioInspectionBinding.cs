
using UnityEngine;

public class ScenarioInspectionBinding : MonoBehaviour
{
    [SerializeField] private ScenarioRunner _scenarioRunner;
    [SerializeField] private InspectionController _inspectionController;

    private void OnEnable()
    {
        _scenarioRunner.StepStarted += OnStepStarted;

        if (_scenarioRunner.CurrentStep != null)
            OnStepStarted(_scenarioRunner.CurrentStep);
    }

    private void OnDisable()
    {
        _scenarioRunner.StepStarted -= OnStepStarted;
        _inspectionController.SetAllowedObjects(null);
    }

    private void OnStepStarted(Step step)
    {
        _inspectionController.SetAllowedObjects(step.ScenarioObjects);
    }
}
