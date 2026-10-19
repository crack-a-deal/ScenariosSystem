using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceOptionEditorView : MonoBehaviour
{
    public event Action<string> TextChanged;
    public event Action<bool> ValueChanged;
    public event Action RemoveClicked;

    [SerializeField] private Toggle toggle;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button removeButton;

    private void Awake()
    {
        toggle.onValueChanged.AddListener(OnValueChanged);
        inputField.onValueChanged.AddListener(OnTextChanged);
        removeButton.onClick.AddListener(OnRemoveClicked);
    }

    public void SetText(string text)
    {
        inputField.SetTextWithoutNotify(text);
    }

    public void SetValue(bool value)
    {
        toggle.SetIsOnWithoutNotify(value);
    }

    private void OnTextChanged(string value)
    {
        TextChanged?.Invoke(value);
    }

    private void OnValueChanged(bool value)
    {
        ValueChanged?.Invoke(value);
    }

    private void OnRemoveClicked()
    {
        RemoveClicked?.Invoke();
    }
}
