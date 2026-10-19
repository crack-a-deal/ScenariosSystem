using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModuleItemView : MonoBehaviour
{
    public event Action<ModuleItemView> ItemClicked;

    [SerializeField] private TMP_Text indexLabel;
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private Button itemButton;
    [SerializeField] private Toggle toggle;

    private void Awake()
    {
        itemButton.onClick.AddListener(OnItemClicked);
    }

    private void OnDestroy()
    {
        itemButton.onClick.RemoveListener(OnItemClicked);
    }

    public void SetIndex(string value)
    {
        indexLabel.text = value;
    }

    public void SetTitle(string value)
    {
        titleLabel.text = value;
    }

    private void OnItemClicked()
    {
        ItemClicked?.Invoke(this);
    }
}
