using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class Scenario : MonoBehaviour
{
    [Title("Scenario")]
    [SerializeField]
    private string _id;

    [SerializeField]
    private string _title;

    [ListDrawerSettings(ShowFoldout = true, DraggableItems = true, ShowIndexLabels = true)]
    [SerializeField]
    private List<Step> _steps = new();

    public string Id => _id;
    public string Title => _title;
    public IReadOnlyList<Step> Steps => _steps;
}