using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceQuestionView : MonoBehaviour
{
    public event Action<Guid, bool> OptionValueChanged;

    [SerializeField] private TMP_Text questionText;
    [SerializeField] private List<ChoiceOptionView> optionViews;

    [SerializeField] private ToggleGroup group;

    private void Awake()
    {
        foreach (var item in optionViews)
        {
            item.ValueChanged += Item_ValueChanged;
        }
    }

    private void OnDestroy()
    {
        foreach (var item in optionViews)
        {
            item.ValueChanged -= Item_ValueChanged;
        }
    }

    private void Item_ValueChanged(ChoiceOptionView option, bool value)
    {
        Debug.Log($"{option.OptionId} is {value}",option.gameObject);
        OptionValueChanged?.Invoke(option.OptionId, value);

        //for (int i = 0; i < optionViews.Count; i++)
        //{
        //    if (optionViews[i] != option)
        //        continue;

        //    OptionValueChanged?.Invoke(option.OptionId,value);
        //    break;
        //}
    }

    public void SetQuestionText(string text)
    {
        questionText.text = text;
    }

    public void SetOptions(IReadOnlyList<AnswerOptionDTO> options)
    {
        for (int i = 0; i < optionViews.Count; i++)
        {
            ChoiceOptionView item = optionViews[i];

            if (i < options.Count)
            {
                item.gameObject.SetActive(true);
                item.SetOption(options[i].Id, options[i].Text);
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        }

        if (options.Count > optionViews.Count)
        {
            Debug.LogError(
                $"ChoiceQuestionView: у вопроса {options.Count} вариантов, " +
                $"в сцене {optionViews.Count} слотов — лишние варианты не показаны.");
        }
    }

    public void SetToggleGroup()
    {
        for (int i = 0; i < optionViews.Count; i++)
        {
            ChoiceOptionView item = optionViews[i];
            item.SetToggleGroup(group);
        }
    }
}
