using UnityEngine;

public class InspectionModelInfo : MonoBehaviour
{
    [SerializeField] private string _title;
    [TextArea(3, 8)]
    [SerializeField] private string _description;

    public string Title => _title;
    public string Description => _description;
}