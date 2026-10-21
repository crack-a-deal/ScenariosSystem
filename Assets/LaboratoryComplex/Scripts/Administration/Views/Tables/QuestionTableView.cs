using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionTableView : MonoBehaviour
{
    public event Action<QuestionType> AddQuestionClicked;

    [SerializeField] private Transform content;
    [SerializeField] private QuestionRowView rowPrefab;
    [SerializeField] private TMP_Dropdown questionsDropdown;
    [SerializeField] private TMP_Dropdown questionTypeDropdown;
    [SerializeField] private Button addQuestionButton;

    public int SelectedAvailableQuestionIndex =>questionsDropdown.value - 1;

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

    public void SetAvailableQuestions(List<BaseQuestionDTO> questions)
    {
        questionsDropdown.ClearOptions();

        var options = new List<TMP_Dropdown.OptionData>
    {
        new TMP_Dropdown.OptionData("Выберите вопрос")
    };

        if (questions != null)
        {
            foreach (var question in questions)
            {
                options.Add(new TMP_Dropdown.OptionData(
    TruncateText(question.Text, 55)
));
            }
        }

        questionsDropdown.AddOptions(options);
        questionsDropdown.SetValueWithoutNotify(0);
        questionsDropdown.RefreshShownValue();
    }

    public void ResetAvailableQuestionsSelection()
    {
        questionsDropdown.SetValueWithoutNotify(0);
        questionsDropdown.RefreshShownValue();
    }

    private string TruncateText(string text, int maxLength = 30)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;

        return text.Substring(0, maxLength) + "...";
    }

    public void ClearAvailableQuestions()
    {
        questionsDropdown.ClearOptions();
        questionsDropdown.AddOptions(
            new List<string> { "Загрузка вопросов..." }
        );
        questionsDropdown.interactable = false;
    }

    public void EnableQuestionsDropdown()
    {
        questionsDropdown.interactable = true;
    }

    private void OnAddQuestionClicked()
    {
        QuestionType type = (QuestionType)questionTypeDropdown.value;
        AddQuestionClicked?.Invoke(type);
    }
}
