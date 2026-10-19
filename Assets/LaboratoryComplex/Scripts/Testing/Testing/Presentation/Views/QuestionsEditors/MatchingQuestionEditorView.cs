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
