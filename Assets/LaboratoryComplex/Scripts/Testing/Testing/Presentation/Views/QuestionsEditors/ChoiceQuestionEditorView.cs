
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceQuestionEditorView : MonoBehaviour
{
    public event Action<string> QuestionTextChanged;
    public event Action AddOptionClicked;
    public event Action<Guid> RemoveOptionClicked;
    public event Action<Guid, bool> OptionValueChanged;
    public event Action SaveClicked;
    public event Action CancelClicked;
    public event Action<Guid, string> OptionTextChanged;

    [SerializeField] private TMP_InputField questionInput;
    [SerializeField] private Transform optionsContainer;
    [SerializeField] private ChoiceOptionEditorView optionPrefab;
    [SerializeField] private Button addOptionButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button cancelButton;

    private readonly List<ChoiceOptionEditorView> _optionViews = new();
    private readonly Dictionary<ChoiceOptionEditorView, Guid> _optionIds = new();

    private bool _singleSelection;

    private void Awake()
    {
        questionInput.onValueChanged.AddListener(OnQuestionTextChanged);
        addOptionButton.onClick.AddListener(OnAddOptionClicked);
        saveButton.onClick.AddListener(OnSaveClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
    }

    public void SetSelectionMode(bool singleSelection)
    {
        _singleSelection = singleSelection;
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

    public void SetQuestionText(string text)
    {
        questionInput.SetTextWithoutNotify(text);
    }

    public void ClearOptions()
    {
        foreach (var optionView in _optionViews)
        {
            Destroy(optionView.gameObject);
        }

        _optionViews.Clear();
        _optionIds.Clear();
    }

    public void AddOption(Guid id, string text, bool value)
    {
        var optionView = Instantiate(optionPrefab, optionsContainer);

        optionView.SetText(text);
        optionView.SetValue(value);

        optionView.TextChanged += textValue =>
            OptionTextChanged?.Invoke(id, textValue);

        optionView.ValueChanged += isOn =>
            HandleOptionValueChanged(optionView, id, isOn);

        optionView.RemoveClicked += () =>
            RemoveOptionClicked?.Invoke(id);

        _optionViews.Add(optionView);
        _optionIds[optionView] = id;

        if (_singleSelection && value)
        {
            foreach (var other in _optionViews)
            {
                if (other == optionView)
                    continue;

                other.SetValue(false);
            }
        }
    }

    private void HandleOptionValueChanged(
        ChoiceOptionEditorView selectedView,
        Guid id,
        bool isOn)
    {
        if (_singleSelection && isOn)
        {
            foreach (var optionView in _optionViews)
            {
                if (optionView == selectedView)
                    continue;

                optionView.SetValue(false);
            }
        }

        OptionValueChanged?.Invoke(id, isOn);
    }
}
