using System;
using UnityEngine;
using UnityEngine.UI;

public class MainView : MonoBehaviour
{
    public event Action CloseClicked;

    [SerializeField] private Button closeButton;

    private void Awake()
    {
        closeButton.onClick.AddListener(OnCloseClicked);
    }

    private void OnDestroy()
    {
        closeButton.onClick.RemoveListener(OnCloseClicked);
    }

    private void OnCloseClicked()
    {
        CloseClicked?.Invoke();
    }
}
