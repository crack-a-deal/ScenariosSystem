using System;
using UnityEngine;

public class InspectionSelectionController : MonoBehaviour
{
    public InspectionPart SelectedPart { get; private set; }

    public event Action<InspectionPart> PartSelected;

    public void SelectPart(InspectionPart part)
    {
        if (part == null || SelectedPart == part)
            return;

        if (SelectedPart != null)
            SelectedPart.SetHighlight(false);

        SelectedPart = part;
        SelectedPart.SetHighlight(true);
        PartSelected?.Invoke(SelectedPart);
    }

    public void VisiblePart(InspectionPart part)
    {
        part.SetVisible(!part.gameObject.activeSelf);
    }
}
