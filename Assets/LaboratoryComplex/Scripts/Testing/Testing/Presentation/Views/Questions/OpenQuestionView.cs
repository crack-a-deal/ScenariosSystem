using System;
using TMPro;
using UnityEngine;

public class OpenQuestionView : MonoBehaviour
{
    public event Action<string> EndEdit;

    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_InputField inputField;

    private void Awake()
    {
        inputField.onEndEdit.AddListener(OnEndEdit);
    }

    private void OnDestroy()
    {
        inputField.onEndEdit.RemoveListener(OnEndEdit);
    }

    public void SetQuestionText(string text)
    {
        questionText.text = text;
    }

    private void OnEndEdit(string input)
    {
        EndEdit?.Invoke(input);
    }
}
