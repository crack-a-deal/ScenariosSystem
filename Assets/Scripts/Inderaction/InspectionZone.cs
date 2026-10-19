using UnityEngine;

[RequireComponent(typeof(ScenarioStateProvider))]
public class InspectionZone : MonoBehaviour
{
    [SerializeField] private float _inspectionDuration = 1f;
    [SerializeField] private string _inspectionText;

    private ScenarioStateProvider _stateProvider;

    public float InspectionDuration => _inspectionDuration;
    public string InspectionText => _inspectionText;

    private void Awake()
    {
        _stateProvider = GetComponent<ScenarioStateProvider>();
    }

    public void CompleteInspection(ScenarioObjectState state)
    {
        _stateProvider.SetState(state);
    }
}