using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StudentRowView : MonoBehaviour
{
    public event Action<StudentRowButton> ButtonClicked;

    public enum StudentRowButton
    {
        Result,
        Edit,
        Delete
    }

    [SerializeField] private TMP_Text indexLabel;
    [SerializeField] private TMP_Text fullNameLabel;
    [SerializeField] private TMP_Text groupLabel;

    [SerializeField] private Button resultButton;
    [SerializeField] private Button editButton;
    [SerializeField] private Button deleteButton;

    private void Awake()
    {
        resultButton.onClick.AddListener(
            () => ButtonClicked?.Invoke(StudentRowButton.Result));

        editButton.onClick.AddListener(
            () => ButtonClicked?.Invoke(StudentRowButton.Edit));

        deleteButton.onClick.AddListener(
            () => ButtonClicked?.Invoke(StudentRowButton.Delete));
    }

    private void OnDestroy()
    {
        resultButton.onClick.RemoveListener(
            () => ButtonClicked?.Invoke(StudentRowButton.Result));

        editButton.onClick.RemoveListener(
            () => ButtonClicked?.Invoke(StudentRowButton.Edit));

        deleteButton.onClick.RemoveListener(
            () => ButtonClicked?.Invoke(StudentRowButton.Delete));
    }

    public void SetIndex(string value)
    {
        indexLabel.text = value;
    }

    public void SetFullName(string value)
    {
        fullNameLabel.text = value;
    }

    public void SetGroup(string value)
    {
        groupLabel.text = value;
    }
}
