using UnityEngine;

[CreateAssetMenu(fileName = "ModulesList", menuName = "ScriptableObjects/ModulesList")]
public class ModulesList : ScriptableObject
{
    [SerializeField] private string[] _modules;

    public string[] Modules => _modules;
}
