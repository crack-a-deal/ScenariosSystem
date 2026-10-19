using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdminTablesView : MonoBehaviour
{
    public event Action<AdminTable> TableSelected;
    public event Action AddClicked;

    [SerializeField] private TMP_Text _tableLabel;

    [SerializeField] private Button _computersButton;
    [SerializeField] private Button _groupsButton;
    [SerializeField] private Button _studentsButton;
    [SerializeField] private Button _testsButton;

    [SerializeField] private Button _addButton;
    [SerializeField] private GameObject _studentWindow;
    [SerializeField] private GameObject _groupWindow;


    private void Awake()
    {
        _computersButton.onClick.AddListener(() => TableSelected?.Invoke(AdminTable.Computers));
        _groupsButton.onClick.AddListener(() => TableSelected?.Invoke(AdminTable.Groups));
        _studentsButton.onClick.AddListener(() => TableSelected?.Invoke(AdminTable.Students));
        _testsButton.onClick.AddListener(() => TableSelected?.Invoke(AdminTable.Tests));

        _addButton.onClick.AddListener(() => AddClicked?.Invoke());
    }

    private void OnDestroy()
    {
        _computersButton.onClick.RemoveListener(() => TableSelected?.Invoke(AdminTable.Computers));
        _groupsButton.onClick.RemoveListener(() => TableSelected?.Invoke(AdminTable.Groups));
        _studentsButton.onClick.RemoveListener(() => TableSelected?.Invoke(AdminTable.Students));
        _testsButton.onClick.RemoveListener(() => TableSelected?.Invoke(AdminTable.Tests));

        _addButton.onClick.RemoveListener(() => AddClicked?.Invoke());
    }

    public void SetTableLabel(string label)
    {
        _tableLabel.text = label;
    }

    public void SetAddButtonVisible(bool visible)
    {
        _addButton.gameObject.SetActive(visible);
    }
}
