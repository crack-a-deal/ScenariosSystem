using UnityEngine;

public class TestsTableView : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private TestRowView rowPrefab;

    public TestRowView CreateRow()
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
