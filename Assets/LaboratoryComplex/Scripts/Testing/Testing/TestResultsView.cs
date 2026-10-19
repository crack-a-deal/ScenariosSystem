using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestResultsView : MonoBehaviour
{
    public event Action ContinueClicked;
    [SerializeField] private TMP_Text _resultText;
    [SerializeField] private Button _continueButton;


    private void Awake()
    {
        _continueButton.onClick.AddListener(OnContinueClicked);
    }

    public void Show(string message)
    {
        _resultText.text = message;
    }


    private void OnContinueClicked()
    {
        ContinueClicked?.Invoke();
    }

    private void OnDestroy()
    {
        _continueButton.onClick.RemoveListener(OnContinueClicked);
    }
}
