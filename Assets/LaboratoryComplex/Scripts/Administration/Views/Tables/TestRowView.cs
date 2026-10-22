using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestRowView : MonoBehaviour
{
    public enum TestRowButton
    {
        AssignStudent,
        AssignAll,
        Edit,
        Delete
    }

    [SerializeField] private TMP_Text indexLabel;
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private TMP_Text questionCountLabel;

    [SerializeField] private Button assingStudentButton;

    [SerializeField] private Button assingAllButton;
    [SerializeField] private Button editButton;
    [SerializeField] private Button deleteButton;

    public event Action<TestRowButton> ButtonClicked;

    private void Awake()
    {
        assingStudentButton.onClick.AddListener(OnAssignStudentClicked);
        assingAllButton.onClick.AddListener(OnAssignAllClicked);
        editButton.onClick.AddListener(OnEditClicked);
        deleteButton.onClick.AddListener(OnDeleteClicked);
    }

    private void OnDestroy()
    {
        assingStudentButton.onClick.RemoveListener(OnAssignStudentClicked);
        assingAllButton.onClick.RemoveListener(OnAssignAllClicked);
        editButton.onClick.RemoveListener(OnEditClicked);
        deleteButton.onClick.RemoveListener(OnDeleteClicked);
    }

    private void OnAssignStudentClicked()
    {
        ButtonClicked?.Invoke(TestRowButton.AssignStudent);
    }

    private void OnAssignAllClicked()
    {
        ButtonClicked?.Invoke(TestRowButton.AssignAll);
    }

    private void OnEditClicked()
    {
        ButtonClicked?.Invoke(TestRowButton.Edit);
    }

    private void OnDeleteClicked()
    {
        ButtonClicked?.Invoke(TestRowButton.Delete);
    }

    public void SetIndex(string value)
    {
        indexLabel.text = value;
    }

    public void SetTitle(string value)
    {
        titleLabel.text = value;
    }

    public void SetQuestioCount(string value)
    {
        questionCountLabel.text = value;
    }
}