using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceOptionView : MonoBehaviour
{
    public event Action<ChoiceOptionView, bool> ValueChanged;

    [SerializeField] private Toggle toggle;
    [SerializeField] private TMP_Text label;

    private Guid _optionId;

    public Guid OptionId => _optionId;

    private void OnEnable()
    {
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    private void OnDisable()
    {
        toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
    }

    public void SetOption(Guid optionId, string optionText, bool isSelected = false)
    {
        label.text = optionText;
        _optionId = optionId;
        toggle.isOn = isSelected;
    }

    public void SetToggleGroup(ToggleGroup group)
    {
        toggle.group = group;
    }

    private void OnToggleValueChanged(bool isOn)
    {
        Debug.Log($"{isOn} is {gameObject.name}", gameObject);
        if (isOn)
        {
            ValueChanged?.Invoke(this, isOn);
        }
    }
}
