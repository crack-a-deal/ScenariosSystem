using System.Collections.Generic;
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
        _currentProvider = null;
        _inspectionProgress = 0f;
        _indicator.Hide();

        if (target == null)
            return;
        var provider = target.GetComponent<ScenarioStateProvider>();
        if (provider == null || !_allowedObjects.Contains(provider))
            return;

        var inspection = target.GetComponent<InspectionZone>();

        if (inspection == null)
            return;

        _currentProvider = provider;
        _currentInspection = inspection;

        _indicator.Show(_displayText + '\n' + inspection.InspectionText);
    }

    public void SetHighlighting(bool enabled)
    {
        _useHighlighting = enabled;

        foreach (var provider in _allowedObjects)
        {
            if (provider != null)
                provider.SetHighlighted(enabled);
        }
    }

    public void DisableHighlighting(ScenarioStateProvider provider)
    {
        if (provider == null)
            return;

        provider.SetHighlighted(false);
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
            DisableHighlighting(_currentProvider);

        //if (_objectState == ScenarioObjectState.Inspected || _objectState == ScenarioObjectState.Loosened || _objectState == ScenarioObjectState.Removed || _objectState == ScenarioObjectState.Marked)

        _indicator.Hide();

        _currentInspection = null;
        _currentProvider = null;
    }

    private bool _useHighlighting = true;

    private readonly HashSet<ScenarioStateProvider> _allowedObjects = new();
    private ScenarioStateProvider _currentProvider;

    public void SetAllowedObjects(IEnumerable<ScenarioStateProvider> objects)
    {
        foreach (var provider in _allowedObjects)
        {
            if (provider != null)
                provider.SetHighlighted(false);
        }

        _allowedObjects.Clear();
        _currentProvider = null;
        _currentInspection = null;
        _inspectionProgress = 0f;
        _indicator.Hide();

        if (objects == null)
            return;

        foreach (var provider in objects)
        {
            if (provider == null)
                continue;

            _allowedObjects.Add(provider);

            if (_useHighlighting)
                provider.SetHighlighted(true);
        }
    }
}