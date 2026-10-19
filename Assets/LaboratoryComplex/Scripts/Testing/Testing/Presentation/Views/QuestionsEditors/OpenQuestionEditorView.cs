using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpenQuestionEditorView : MonoBehaviour
{
    public event Action<string> QuestionTextChanged;
    public event Action<string> CorrectAnswerChanged;

    public event Action SaveClicked;
    public event Action CancelClicked;

    [SerializeField] private TMP_InputField questionInput;
    [SerializeField] private TMP_InputField correctAnswerInput;

    [SerializeField] private Button saveButton;
    [SerializeField] private Button cancelButton;

    private void Awake()
    {
        questionInput.onValueChanged.AddListener(OnQuestionTextChanged);

        correctAnswerInput.onValueChanged.AddListener(OnCorrectAnswerChanged);

        saveButton.onClick.AddListener(OnSaveClicked);

        cancelButton.onClick.AddListener(OnCancelClicked);
    }

    public void SetQuestionText(string text)
    {
        questionInput.SetTextWithoutNotify(text);
    }

    public void SetCorrectAnswer(string text)
    {
        correctAnswerInput.SetTextWithoutNotify(text);
    }

    private void OnQuestionTextChanged(string value)
    {
        QuestionTextChanged?.Invoke(value);
    }

    private void OnCorrectAnswerChanged(string value)
    {
        CorrectAnswerChanged?.Invoke(value);
    }

    private void OnSaveClicked()
    {
        SaveClicked?.Invoke();
    }

    private void OnCancelClicked()
    {
        CancelClicked?.Invoke();
    }
}
