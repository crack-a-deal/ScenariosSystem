using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionTableView : MonoBehaviour
{
    public event Action<QuestionType> AddQuestionClicked;

    [SerializeField] private Transform content;
    [SerializeField] private QuestionRowView rowPrefab;
    [SerializeField] private TMP_Dropdown questionTypeDropdown;
    [SerializeField] private Button addQuestionButton;

    private void OnEnable()
    {
        addQuestionButton.onClick.AddListener(OnAddQuestionClicked);
    }

    private void OnDisable()
    {
        addQuestionButton.onClick.RemoveListener(OnAddQuestionClicked);
    }

    public QuestionRowView CreateRow()
    {
        return Instantiate(rowPrefab, content);
    }

    public void Clear()
    {
        for (var i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }
    }

    private void OnAddQuestionClicked()
    {
        QuestionType type = (QuestionType)questionTypeDropdown.value;
        AddQuestionClicked?.Invoke(type);
    }
}
