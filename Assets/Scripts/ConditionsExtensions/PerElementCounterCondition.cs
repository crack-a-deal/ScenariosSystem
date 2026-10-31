using LBG;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PerElementCounterCondition : ConditionBase, IScenarioObjectsProvider
{
    [ListDrawerSettings(ShowFoldout = true)]
    [SerializeField]
    private List<CounterElement> _elements = new();

    [SerializeField]
    private ScenarioObjectState _requiredState;

    [Tooltip("Сколько элементов достаточно для завершения шага. 0 = все элементы.")]
    [MinValue(0)]
    [SerializeField]
    private int _requiredCount;


    private readonly HashSet<ScenarioStateProvider> _completed = new();
    private List<ScenarioStateProvider> _objectsCache;

    public IReadOnlyList<ScenarioStateProvider> Objects
    {
        get
        {
            if (_objectsCache == null)
            {
                _objectsCache = new List<ScenarioStateProvider>(_elements.Count);

                foreach (var element in _elements)
                {
                    if (element != null && element.Object != null)
                        _objectsCache.Add(element.Object);
                }
            }

            return _objectsCache;
        }
    }

    private int EffectiveRequiredCount => _requiredCount <= 0 ? Objects.Count : _requiredCount;

    [ShowInInspector]
    [ReadOnly]
    private int CompletedCount => _completed.Count;

    [ShowInInspector]
    [ReadOnly]
    private int TotalCount => Objects.Count;

    public override void Reset()
    {
        _completed.Clear();
    }

    public override bool Evaluate(ScenarioContext context, ScenarioEvent scenarioEvent)
    {
        foreach (var element in _elements)
        {
            if (element == null || element.Object == null)
                continue;

            if (_completed.Contains(element.Object))
                continue;

            if (!element.Object.Is(_requiredState))
                continue;

            _completed.Add(element.Object);
            element.Complete(context);
        }

        return _completed.Count >= EffectiveRequiredCount;
    }
}

[Serializable]
public class CounterElement
{
    [Required]
    [SerializeField]
    private ScenarioStateProvider _object;

    [LabelText("Выключить сам объект")]
    [Tooltip("Выключить GameObject самого элемента при выполнении — болт «снят» с двигателя")]
    [SerializeField]
    private bool _disableSelfOnComplete = true;

    [LabelText("Выключить вместе с элементом")]
    [Tooltip("Гайка, шайба и другие объекты, которые должны исчезнуть вместе с элементом")]
    [SerializeField]
    private List<GameObject> _disableOnComplete = new();

    [LabelText("Включить при выполнении (копии)")]
    [Tooltip("Копии на столе — болт, гайка и т.д. — включатся при выполнении элемента")]
    [SerializeField]
    private List<GameObject> _enableOnComplete = new();

    [FoldoutGroup("Доп действия")]
    [SerializeReference, SubclassSelector]
    private List<ScenarioAction> _onComplete = new();

    public ScenarioStateProvider Object => _object;

    public void Complete(ScenarioContext context)
    {
        if (_disableSelfOnComplete && _object != null)
            _object.gameObject.SetActive(false);

        foreach (GameObject target in _disableOnComplete)
        {
            if (target != null)
                target.SetActive(false);
        }

        foreach (GameObject target in _enableOnComplete)
        {
            if (target != null)
                target.SetActive(true);
        }

        foreach (var action in _onComplete)
            action?.Execute(context);
    }
}