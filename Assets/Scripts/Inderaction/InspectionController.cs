using UnityEngine;

public class InspectionController : MonoBehaviour
{
    [SerializeField] private CursorPointer _cursorPointer;
    [SerializeField] private InspectionIndicator _indicator;
    private InputSystem_Actions _inputActions;

    private InspectionZone _currentInspection;
    private float _inspectionProgress;

    private string _displayText = "Зона визуального контроля";
    private ScenarioObjectState _objectState = ScenarioObjectState.Inspected;

    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _inputActions.Enable();
        _cursorPointer.ObjectChanged += OnObjectChanged;
    }

    private void OnDisable()
    {
        _inputActions.Disable();
        _cursorPointer.ObjectChanged -= OnObjectChanged;
    }

    private void Update()
    {
        UpdateInspection();
    }

    public void SetText(string text)
    {
        _displayText = text;
    }

    public void SetInspectState()
    {
        _objectState = ScenarioObjectState.Inspected;
    }
    public void SetMarkedState()
    {
        _objectState = ScenarioObjectState.Marked;
    }
    public void SetLoosenedState()
    {
        _objectState = ScenarioObjectState.Loosened;
    }

    public void SetRemovedState()
    {
        _objectState = ScenarioObjectState.Removed;
    }

    private void OnObjectChanged(GameObject target)
    {
        _currentInspection = null;
        _inspectionProgress = 0f;
        _indicator.Hide();

        if (target == null)
            return;

        _currentInspection = target.GetComponentInParent<InspectionZone>();

        if (_currentInspection == null)
            return;

        _indicator.Show(_displayText + '\n' + _currentInspection.InspectionText);
    }

    private void UpdateInspection()
    {
        if (_currentInspection == null)
            return;

        if (!_inputActions.Player.Interact.IsPressed())
        {
            _inspectionProgress = 0f;
            _indicator.SetProgress(0f);
            return;
        }

        _inspectionProgress += Time.deltaTime / _currentInspection.InspectionDuration;
        _indicator.SetProgress(_inspectionProgress);

        if (_inspectionProgress < 1f)
            return;

        _currentInspection.CompleteInspection(_objectState);

        _indicator.Hide();
        _currentInspection = null;
    }
}