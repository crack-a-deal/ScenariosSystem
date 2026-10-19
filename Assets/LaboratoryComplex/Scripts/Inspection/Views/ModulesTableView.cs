using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModulesTableView : MonoBehaviour
{
    public event Action BackClicked;

    [SerializeField] private Transform content;
    [SerializeField] private ModuleItemView rowPrefab;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text message;


    private void Awake()
    {
        backButton.onClick.AddListener(OnBackClicked);
    }

    private void OnDestroy()
    {
        backButton.onClick.RemoveListener(OnBackClicked);
    }


    public ModuleItemView CreateRow()
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

    public void SetMessage(string value)
    {
        message.SetText(value);
    }

    public void ActiveteMessage(bool value)
    {
        message.enabled = value;
    }

    private void OnBackClicked()
    {
        BackClicked?.Invoke();
    }
}