using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GroupFormView : MonoBehaviour
{
    public event Action SubmitClicked;
    public event Action CancelClicked;

    [SerializeField] private TMP_InputField numberInput;
    [SerializeField] private TMP_InputField courseInput;
    [SerializeField] private ScrollRect list;
    [SerializeField] private Transform content;
    [SerializeField] private StudentRowView itemPrefab;
    [SerializeField] private TMP_Text listText;


    [SerializeField] private Button submitButton;
    [SerializeField] private Button cancelButton;

    private void Awake()
    {
        submitButton.onClick.AddListener(OnSubmitClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
    }

    private void OnDestroy()
    {
        submitButton.onClick.RemoveListener(OnSubmitClicked);
        cancelButton.onClick.RemoveListener(OnCancelClicked);
    }

    private void OnSubmitClicked()
    {
        SubmitClicked?.Invoke();
    }

    private void OnCancelClicked()
    {
        CancelClicked?.Invoke();
    }

    public string GetNumberName()
    {
        return numberInput.text;
    }

    public void SetNumberName(string number)
    {
        numberInput.text = number;
    }

    public string GetCourse()
    {
        return courseInput.text;
    }

    public void SetCourse(string course)
    {
        courseInput.text = course;
    }

    public void ClearList()
    {
        for (var i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }
    }

    public void SetListText(string text)
    {
        listText.SetText(text);
    }

    public void ListTextActivate(bool isActive)
    {
        listText.gameObject.SetActive(isActive);
    }

    public StudentRowView CreateRow()
    {
        return Instantiate(itemPrefab, content);
    }

    public void Clear()
    {
        numberInput.text = string.Empty;
        courseInput.text = string.Empty;
    }
}
