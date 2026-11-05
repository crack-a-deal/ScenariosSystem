using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchingQuestionEditorView : MonoBehaviour
{
    public event Action<string> QuestionTextChanged;

    public event Action AddPairClicked;

    public event Action SaveClicked;
    public event Action CancelClicked;

    [SerializeField] private TMP_InputField questionInput;

    [SerializeField] private ReordableListView leftList;
    [SerializeField] private ReordableListView rightList;

    [SerializeField] private Button addPairButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TMP_Text errorText;

    public ReordableListView LeftList => leftList;
    public ReordableListView RightList => rightList;

    private void Awake()
    {
        questionInput.onValueChanged.AddListener(OnQuestionTextChanged);
        addPairButton.onClick.AddListener(OnAddPairClicked);
        saveButton.onClick.AddListener(OnSaveClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
    }

    public void SetQuestionText(string text)
    {
        questionInput.SetTextWithoutNotify(text);
    }



    public void ShowError(string message)
    {
        if (errorText == null)
            return;

        errorText.text = message;
        errorText.gameObject.SetActive(true);
    }

    public void ClearError()
    {
        if (errorText == null)
            return;

        errorText.text = string.Empty;
        errorText.gameObject.SetActive(false);
    }

    private void OnQuestionTextChanged(string value)
    {
        QuestionTextChanged?.Invoke(value);
    }

    private void OnAddPairClicked()
    {
        AddPairClicked?.Invoke();
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
