using UnityEngine;

public class GroupsTableView : MonoBehaviour
{
    [SerializeField] private Transform _content;
    [SerializeField] private GroupRowView _rowPrefab;

    public GroupRowView CreateRow()
    {
        return Instantiate(_rowPrefab, _content);
    }

    public void Clear()
    {
        for (var i = _content.childCount - 1; i >= 0; i--)
        {
            Destroy(_content.GetChild(i).gameObject);
        }
    }
}
