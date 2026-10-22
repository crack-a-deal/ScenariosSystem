
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StudentAssignmentPanelView : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown studentsDropdown;
    [SerializeField] private Button assignButton;
    [SerializeField] private Button cancelButton;

    public event Action AssignClicked;
    public event Action CancelClicked;

    public int SelectedIndex => studentsDropdown.value;

    private void Awake()
    {
        assignButton.onClick.AddListener(OnAssignClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
    }

    private void OnDestroy()
    {
        assignButton.onClick.RemoveListener(OnAssignClicked);
        cancelButton.onClick.RemoveListener(OnCancelClicked);
    }

    private void OnAssignClicked()
    {
        AssignClicked?.Invoke();
    }

    private void OnCancelClicked()
    {
        CancelClicked?.Invoke();
    }

    public void SetStudents(List<string> names)
    {
        studentsDropdown.ClearOptions();
        studentsDropdown.AddOptions(names);
        studentsDropdown.SetValueWithoutNotify(0);
        studentsDropdown.RefreshShownValue();
    }

    public void SetInteractable(bool value)
    {
        assignButton.interactable = value;
        studentsDropdown.interactable = value;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}

