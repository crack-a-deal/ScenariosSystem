
using System;
using System.Collections.Generic;

public class ViewedModelExcelDto
{
    public int Number { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public DateTime ViewedAt { get; set; }
}

public class ViewedModelsExportDTO
{
    public List<UserModelViewDto> Views { get; set; } = new();
}