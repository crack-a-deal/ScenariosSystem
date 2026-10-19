using System;
using UnityEngine;
using UnityEngine.UI;

public class AdministrationPanelView : MonoBehaviour
{
    public event Action BackClicked;

    [SerializeField] private Button _backButton;

    private void OnEnable()
    {
        _backButton.onClick.AddListener(OnBackClicked);
    }

    private void OnDisable()
    {
        _backButton.onClick.RemoveListener(OnBackClicked);
    }

    private void OnBackClicked()
    {
        BackClicked?.Invoke();
    }
}

