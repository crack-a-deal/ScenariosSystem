using System;
using UnityEngine;
using UnityEngine.UI;

public class LearningSectionsView : MonoBehaviour
{
    public event Action BackClicked;

    public event Action InspectionSectionClicked;
    public event Action VirtuaSectionClicked;
    public event Action TestingSectionClicked;

    [SerializeField] private Button inspectionSection;
    [SerializeField] private Button virtualSection;
    [SerializeField] private Button testingSection;
    [SerializeField] private Button backButton;

    private void Awake()
    {
        inspectionSection.onClick.AddListener(OnInspectionSectionClicked);
        virtualSection.onClick.AddListener(OnVirtuaSectionClicked);
        testingSection.onClick.AddListener(OnTestingSectionClicked);
        backButton.onClick.AddListener(OnBackClicked);
    }

    private void OnDestroy()
    {
        inspectionSection.onClick.RemoveListener(OnInspectionSectionClicked);
        virtualSection.onClick.RemoveListener(OnVirtuaSectionClicked);
        testingSection.onClick.RemoveListener(OnTestingSectionClicked);
        backButton.onClick.RemoveListener(OnBackClicked);
    }

    private void OnInspectionSectionClicked()
    {
        InspectionSectionClicked?.Invoke();
    }

    private void OnVirtuaSectionClicked()
    {
        VirtuaSectionClicked?.Invoke();
    }

    private void OnTestingSectionClicked()
    {
        TestingSectionClicked?.Invoke();
    }

    private void OnBackClicked()
    {
        BackClicked?.Invoke();
    }
}
