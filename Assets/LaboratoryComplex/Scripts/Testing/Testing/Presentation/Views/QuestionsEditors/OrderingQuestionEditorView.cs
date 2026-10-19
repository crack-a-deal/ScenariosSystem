using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderingQuestionEditorView : MonoBehaviour
{
    public event Action<string> QuestionTextChanged;

    public event Action AddOptionClicked;
    public event Action SaveClicked;
    public event Action CancelClicked;

    [SerializeField] private TMP_InputField questionInput;

    [SerializeField] private ReordableListView optionList;

    [SerializeField] private Button addOptionButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button cancelButton;

    public ReordableListView OptionList => optionList;

    private void Awake()
    {
        questionInput.onValueChanged.AddListener(OnQuestionTextChanged);
        addOptionButton.onClick.AddListener(OnAddOptionClicked);
        saveButton.onClick.AddListener(OnSaveClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
    }

    public void SetQuestionText(string text)
    {
        questionInput.SetTextWithoutNotify(text);
    }

    private void OnQuestionTextChanged(string value)
    {
        QuestionTextChanged?.Invoke(value);
    }

    private void OnAddOptionClicked()
    {
        AddOptionClicked?.Invoke();
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
