using System;
using System.Collections.Generic;

public class ExcelExportRegistry
{
    private readonly Dictionary<Type, IExcelExportDefinition> _definitions;

    public ExcelExportRegistry()
    {
        _definitions =
            new Dictionary<Type, IExcelExportDefinition>();
    }

    public void Register(
        IExcelExportDefinition definition)
    {
        if (definition == null)
            throw new ArgumentNullException(nameof(definition));

        _definitions.Add(
            definition.DtoType,
            definition);
    }

    public IExcelExportDefinition GetDefinition(
        Type dtoType)
    {
        if (!_definitions.TryGetValue(
                dtoType,
                out var definition))
        {
            throw new NotSupportedException(
                $"DTO {dtoType.Name} не поддерживает Excel export.");
        }

        return definition;
    }
}