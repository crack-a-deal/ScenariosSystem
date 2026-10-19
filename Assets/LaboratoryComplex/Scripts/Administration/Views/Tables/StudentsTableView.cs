using UnityEngine;

public class StudentsTableView : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private StudentRowView rowPrefab;

    public StudentRowView CreateRow()
    {
        return Instantiate(rowPrefab, content);
    }

    public void Clear()
    {
        for (var i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }
    }
}
