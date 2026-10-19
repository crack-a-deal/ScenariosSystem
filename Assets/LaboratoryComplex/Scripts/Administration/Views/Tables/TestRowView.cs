using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestRowView : MonoBehaviour
{
    public enum TestRowButton
    {
        Assign,
        Edit,
        Delete
    }

    [SerializeField] private TMP_Text indexLabel;
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private TMP_Text questionCountLabel;
    [SerializeField] private TMP_Text timeLabel;

    [SerializeField] private Button assingButton;
    [SerializeField] private Button editButton;
    [SerializeField] private Button deleteButton;

    public event Action<TestRowButton> ButtonClicked;

    private void Awake()
    {
        assingButton.onClick.AddListener(
            () =>
            {
                ButtonClicked?.Invoke(TestRowButton.Assign);
            });
        editButton.onClick.AddListener(
            () =>
            {
                ButtonClicked?.Invoke(TestRowButton.Edit);
            });

        deleteButton.onClick.AddListener(
            () =>
            {
                ButtonClicked?.Invoke(TestRowButton.Delete);
            });
    }

    private void OnDestroy()
    {
        assingButton.onClick.RemoveListener(
            () =>
            {
                ButtonClicked?.Invoke(TestRowButton.Assign);
            });
        editButton.onClick.RemoveListener(
            () =>
            {
                ButtonClicked?.Invoke(TestRowButton.Edit);
            });

        deleteButton.onClick.RemoveListener(
            () =>
            {
                ButtonClicked?.Invoke(TestRowButton.Delete);
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

    public void SetQuestioCount(string value)
    {
        questionCountLabel.text = value;
    }

    public void SetTime(string value)
    {
        timeLabel.text = value;
    }
}