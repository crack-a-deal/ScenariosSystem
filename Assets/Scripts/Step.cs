using LBG;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IScenarioObjectsProvider
{
    IReadOnlyList<ScenarioStateProvider> Objects { get; }
}

[Serializable]
public class Step : MonoBehaviour
{
    [HorizontalGroup("Header")]
    [LabelWidth(60)]
    public string Id;

    [HorizontalGroup("Header")]
    [LabelWidth(60)]
    public string Title;

    [BoxGroup("Completion")]
    [SerializeReference]
    [SubclassSelector]
    private ConditionBase _completionCondition;

    [BoxGroup("On Enter")]
    [ListDrawerSettings(ShowFoldout = true)]
    [SerializeReference, SubclassSelector]
    private List<ScenarioAction> _onEnter = new();

    [BoxGroup("On Enter")]
    [SerializeField]
    private UnityEvent _onEnterEvent = new();


    [BoxGroup("On Complete")]
    [ListDrawerSettings(ShowFoldout = true)]
    [SerializeReference, SubclassSelector]
    private List<ScenarioAction> _onComplete = new();

    [BoxGroup("On Complete")]
    [SerializeField]
    private UnityEvent _onCompleteEvent = new();

    [BoxGroup("Hints")]
    [TextArea(2, 5)]
    [SerializeField] private string _hint;
    public string Hint => _hint;

    [BoxGroup("Errors")]
    [ListDrawerSettings(ShowFoldout = true)]
    [SerializeField]
    private List<ErrorRule> _errors = new();


    public IReadOnlyList<ScenarioStateProvider> ScenarioObjects =>
    (_completionCondition as IScenarioObjectsProvider)?.Objects
    ?? System.Array.Empty<ScenarioStateProvider>();

    public bool IsCompleted { get; private set; }

    public void Enter(ScenarioContext context)
    {
        IsCompleted = false;

        _completionCondition?.Reset();

        foreach (var action in _onEnter)
        {
            action.Execute(context);
        }
        _onEnterEvent?.Invoke();
    }

    public void ProcessEvent(ScenarioContext context, ScenarioEvent scenarioEvent, Action<ScenarioError> onError)
    {
        if (IsCompleted)
        {
            return;
        }

        EvaluateErrors(context, scenarioEvent, onError);

        if (_completionCondition != null && _completionCondition.Evaluate(context, scenarioEvent))
        {
            Complete(context);
        }
    }

    private void EvaluateErrors(ScenarioContext context, ScenarioEvent scenarioEvent, Action<ScenarioError> onError)
    {
        foreach (var error in _errors)
        {
            if (!error.Evaluate(context, scenarioEvent))
            {
                continue;
            }

            var scenarioError = error.CreateError();

            context.RegisterError();
            onError?.Invoke(scenarioError);
        }
    }

    private void Complete(ScenarioContext context)
    {
        IsCompleted = true;

        foreach (var action in _onComplete)
        {
            action.Execute(context);
        }
        _onCompleteEvent?.Invoke();
    }
}
