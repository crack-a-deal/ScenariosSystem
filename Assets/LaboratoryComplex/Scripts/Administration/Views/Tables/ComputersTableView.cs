using UnityEngine;

public class ComputersTableView : MonoBehaviour
{
    [SerializeField] private Transform _content;
    [SerializeField] private ComputerRowView _rowPrefab;

    public ComputerRowView CreateRow()
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
