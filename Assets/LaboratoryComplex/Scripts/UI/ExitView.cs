using System;
using UnityEngine;
using UnityEngine.UI;

public class ExitView : MonoBehaviour
{
    public event Action ExitClicked;
    public event Action CancelClicked;

    [SerializeField] private Button exitButton;
    [SerializeField] private Button cancelButton;

    private void Awake()
    {
        exitButton.onClick.AddListener(OnExitClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
    }

    private void OnDestroy()
    {
        exitButton.onClick.RemoveListener(OnExitClicked);
        cancelButton.onClick.RemoveListener(OnCancelClicked);
    }

    private void OnExitClicked()
    {
        ExitClicked?.Invoke();
    }

    private void OnCancelClicked()
    {
        CancelClicked?.Invoke();
    }
}
