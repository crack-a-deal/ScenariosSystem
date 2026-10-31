using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "ScenarioList", menuName = "ScriptableObjects/ScenarioList")]
public class ScenarioList : ScriptableObject
{
    [SerializeField]
    private string[] _scenarios;

    public string[] Scenarios => _scenarios;
}