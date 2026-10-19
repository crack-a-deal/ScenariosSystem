using System;
using System.Collections.Generic;
using UnityEngine;

public class MultiSheetExcelExportRegistry
{
    private readonly Dictionary<Type, IMultiSheetExcelExportDefinition> _definitions;

    public MultiSheetExcelExportRegistry()
    {
        _definitions = new Dictionary<Type, IMultiSheetExcelExportDefinition>();
    }

    public void Register(
        IMultiSheetExcelExportDefinition definition)
    {
        if (definition == null)
            throw new ArgumentNullException(nameof(definition));

        _definitions.Add(
            definition.DtoType,
            definition);
    }

    public IMultiSheetExcelExportDefinition GetDefinition(
        Type dtoType)
    {
        if (!_definitions.TryGetValue(
                dtoType,
                out var definition))
        {
            throw new NotSupportedException(
                $"DTO {dtoType.Name} не поддерживает multi-sheet Excel export.");
        }

        return definition;
    }
}
