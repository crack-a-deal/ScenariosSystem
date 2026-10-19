using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InspectionPartItemView : MonoBehaviour
{
    public event Action<InspectionPart> VisibleButtonClicked;
    public event Action<InspectionPart> SelectButtonClicked;

    [SerializeField] private Button _selectButton;
    [SerializeField] private Button _visibleButton;
    [SerializeField] private Image visibleIcon;
    [SerializeField] private Color visibleColor;
    [SerializeField] private Color hideColor;
    [SerializeField] private TMP_Text _title;
    [SerializeField] private GameObject _selectionIndicator;

    private InspectionPart _part;

    private void Awake()
    {
        _selectButton.onClick.AddListener(OnSelectButtonCLicked);
        _visibleButton.onClick.AddListener(OnVisibleButtonCLicked);
    }

    private void OnDestroy()
    {
        _selectButton.onClick.RemoveListener(OnSelectButtonCLicked);
        _visibleButton.onClick.RemoveListener(OnVisibleButtonCLicked);
    }

    public void Bind(InspectionPart part)
    {
        _part = part;
        _title.text = part.Data.Title;
    }

    public void SetSelected(bool selected)
    {
        if (_selectionIndicator != null)
            _selectionIndicator.SetActive(selected);
    }

    public void SetSelectedFor(InspectionPart part)
    {
        SetSelected(_part == part);
    }

    public void SetVisibilityState(bool visible)
    {
        visibleIcon.color = visible ? visibleColor : hideColor;
    }

    public void SetVisibilityStateFor(InspectionPart part)
    {
        if (_part == part)
            SetVisibilityState(part.IsVisible);
    }

    private void OnSelectButtonCLicked()
    {
        SelectButtonClicked?.Invoke(_part);
    }

    private void OnVisibleButtonCLicked()
    {
        VisibleButtonClicked?.Invoke(_part);
    }
}
