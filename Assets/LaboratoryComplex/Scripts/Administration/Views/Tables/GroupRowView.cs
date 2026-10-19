using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GroupRowView : MonoBehaviour
{
    public event Action<GroupRowButton> ButtonClicked;

    public enum GroupRowButton
    {
        Edit,
        Delete
    }

    [SerializeField] private TMP_Text indexLabel;
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private TMP_Text studentCountLabel;

    [SerializeField] private Button editButton;
    [SerializeField] private Button deleteButton;

    private void Awake()
    {
        editButton.onClick.AddListener(
            () =>
            {
                ButtonClicked?.Invoke(GroupRowButton.Edit);
            });

        deleteButton.onClick.AddListener(
            () =>
            {
                ButtonClicked?.Invoke(GroupRowButton.Delete);
            });
    }

    private void OnDestroy()
    {
        editButton.onClick.RemoveListener(
            () =>
            {
                ButtonClicked?.Invoke(GroupRowButton.Edit);
            });

        deleteButton.onClick.RemoveListener(
            () =>
            {
                ButtonClicked?.Invoke(GroupRowButton.Delete);
            });
    }

    public void SetIndex(string value)
    {
        indexLabel.text = value;
    }

    public void SetTitle(string value)
    {
        titleLabel.text = value;
    }

    public void SetStudentCount(string value)
    {
        studentCountLabel.text = value;
    }
}
