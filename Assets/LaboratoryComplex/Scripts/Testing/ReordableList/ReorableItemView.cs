using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReorableItemView : MonoBehaviour
{
    public event Action<ReorableItemView> UpClicked;
    public event Action<ReorableItemView> DownClicked;
    public event Action<ReorableItemView, string> TextChanged;

    [SerializeField] private TMP_Text indexLabel;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;

    private void OnEnable()
    {
        upButton.onClick.AddListener(OnUpClicked);
        downButton.onClick.AddListener(OnDownClicked);
        inputField.onValueChanged.AddListener(OnTextChanged);
    }

    private void OnDisable()
    {
        upButton.onClick.RemoveListener(OnUpClicked);
        downButton.onClick.RemoveListener(OnDownClicked);
        inputField.onValueChanged.RemoveListener(OnTextChanged);
    }

    public void SetIndexText(string value)
    {
        indexLabel.SetText(value);
    }

    public void SetText(string value)
    {
        inputField.SetTextWithoutNotify(value);
    }

    private void OnUpClicked()
    {
        UpClicked?.Invoke(this);
    }

    private void OnDownClicked()
    {
        DownClicked?.Invoke(this);
    }

    private void OnTextChanged(string value)
    {
        TextChanged?.Invoke(this, value);
    }
}