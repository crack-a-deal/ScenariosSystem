using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InspectionView : MonoBehaviour
{
    public event Action BackClicked;

    [SerializeField] private InspectionPartItemView _partItemPrefab;

    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _description;

    [SerializeField] private Button backButton;

    private readonly List<InspectionPartItemView> _items = new();


    private void Awake()
    {
        backButton.onClick.AddListener(OnBackClicked);
    }

    private void OnDestroy()
    {
        backButton.onClick.RemoveListener(OnBackClicked);
    }

    public void SetModelInfo(string title, string description)
    {
        _title.text = title;
        _description.text = description;
    }

    public void ClearModelInfo()
    {
        _title.text = string.Empty;
        _description.text = string.Empty;
    }

    private void OnBackClicked()
    {
        BackClicked?.Invoke();
    }
}
