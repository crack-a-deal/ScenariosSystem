using System;
using UnityEngine;
using UnityEngine.UI;

public class TestEditorPanelView : MonoBehaviour
{
    public event Action BackClicked;
    public event Action SaveClicked;


    [SerializeField] private Button _backButton;
    [SerializeField] private Button _saveButton;


    private void OnEnable()
    {
        _backButton.onClick.AddListener(OnBackClicked);
        _saveButton.onClick.AddListener(OnSaveClicked);

    }

    private void OnDisable()
    {
        _backButton.onClick.RemoveListener(OnBackClicked);
        _saveButton.onClick.RemoveListener(OnSaveClicked);
    }

    private void OnBackClicked()
    {
        BackClicked?.Invoke();
    }
    private void OnSaveClicked()
    {
        SaveClicked?.Invoke();
    }
}
