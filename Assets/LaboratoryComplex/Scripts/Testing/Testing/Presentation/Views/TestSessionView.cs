using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestSessionView : MonoBehaviour
{
    public event Action SubmitClicked;
    public event Action BackClicked;

    [SerializeField] private TMP_Text questionIndexLabel;
    [SerializeField] private TMP_Text questionTypeLabel;

    [SerializeField] private Button submit;
    [SerializeField] private Button back;


    private void OnEnable()
    {
        submit.onClick.AddListener(OnClicked);
        back.onClick.AddListener(OnBackClicked);
    }

    private void OnDisable()
    {
        submit.onClick.RemoveListener(OnClicked);
        back.onClick.RemoveListener(OnBackClicked);
    }

    public void SetQuestionIndex(string value)
    {
        questionIndexLabel.text = value;
    }

    public void SetQuestionTypeText(string value)
    {
        questionTypeLabel.SetText(value);
    }

    private void OnClicked()
    {
        SubmitClicked?.Invoke();
    }

    private void OnBackClicked()
    {
        BackClicked?.Invoke();
    }
}
