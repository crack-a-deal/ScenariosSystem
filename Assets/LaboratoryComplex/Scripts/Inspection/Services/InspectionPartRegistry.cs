using System.Collections.Generic;
using UnityEngine;

public class InspectionPartRegistry : MonoBehaviour
{
    [SerializeField] private Transform[] models;

    private List<InspectionPart> _parts = new();

    public IReadOnlyList<InspectionPart> Parts => _parts;

    private Transform _selectedModele;
    public Transform SelectedModel => _selectedModele;

    public void Select(string module)
    {
        _selectedModele = null;

        foreach (Transform model in models)
        {
            if (model == null)
                continue;

            bool isSelected = model.gameObject.name == module;

            model.gameObject.SetActive(isSelected);

            if (isSelected)
                _selectedModele = model;
        }

        if (_selectedModele == null)
        {
            Debug.LogError(
                $"InspectionPartRegistry: Model '{module}' not found.",
                this);

            _parts.Clear();
            return;
        }

        Refresh();
    }

    public void Refresh()
    {
        _parts.Clear();

        if (_selectedModele == null)
        {
            Debug.LogError(
                "InspectionPartRegistry: Model Root is not assigned.",
                this);

            return;
        }

        foreach (InspectionPart part in
                 _selectedModele.GetComponentsInChildren<InspectionPart>(true))
        {
            if (part.Data == null)
            {
                Debug.LogWarning(
                    $"InspectionPart '{part.name}' has no data assigned.",
                    part);

                continue;
            }

            _parts.Add(part);
        }
    }
}