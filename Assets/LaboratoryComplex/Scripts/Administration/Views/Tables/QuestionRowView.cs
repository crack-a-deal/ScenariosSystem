using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionRowView : MonoBehaviour
{
    public enum QuestionRowButton
    {
        Edit,
        Delete
    }

    [SerializeField] private TMP_Text indexLabel;
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private TMP_Text typeLabel;


    [SerializeField] private Button editButton;
    [SerializeField] private Button deleteButton;

    public event Action<QuestionRowButton> ButtonClicked;

    private void Awake()
    {
        editButton.onClick.AddListener(
            () =>
            {
                ButtonClicked?.Invoke(QuestionRowButton.Edit);
            });

        deleteButton.onClick.AddListener(
            () =>
            {
                ButtonClicked?.Invoke(QuestionRowButton.Delete);
            });
    }

    private void OnDestroy()
    {
        editButton.onClick.RemoveListener(
            () =>
            {
                ButtonClicked?.Invoke(QuestionRowButton.Edit);
            });

        deleteButton.onClick.RemoveListener(
            () =>
            {
                ButtonClicked?.Invoke(QuestionRowButton.Delete);
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

    public void SetQuestioType(string value)
    {
        typeLabel.text = value;
    }
}