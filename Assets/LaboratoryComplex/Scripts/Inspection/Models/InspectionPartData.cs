using UnityEngine;

[System.Serializable]
public class InspectionPartData
{
    [SerializeField] private string _id;
    [SerializeField] private string _title;
    [TextArea(3, 10)]
    [SerializeField] private string _description;

    public string Id => _id;
    public string Title => _title;
    public string Description => _description;
}