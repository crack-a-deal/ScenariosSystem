using MiniExcelLibs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class ExcelExportService : IExcelExportService
{
    private readonly IExportPathProvider _exportPathProvider;
    private readonly ExcelExportRegistry _registry;
    private readonly MultiSheetExcelExportRegistry _multiSheetRegistry;


    public ExcelExportService(
        ExcelExportRegistry registry,
        MultiSheetExcelExportRegistry multiSheetRegistry,
        IExportPathProvider exportPathProvider)
    {
        _registry = registry;
        _multiSheetRegistry = multiSheetRegistry;
        _exportPathProvider = exportPathProvider;
    }

    public Task ExportAsync<TDto>(
        IEnumerable<TDto> data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        var definition = _registry.GetDefinition(
             typeof(TDto));

        var excelData = data
           .Select(dto => definition.Map(dto))
           .ToList();

        string desktopPath = Environment.GetFolderPath(
            Environment.SpecialFolder.Desktop);

        string filePath =
            _exportPathProvider.GetExportPath(
            definition.FileName);

        filePath = GetUniqueFilePath(filePath);

        MiniExcel.SaveAs(
            filePath,
            excelData);

        Debug.Log($"Excel exported: {filePath}");

        return Task.CompletedTask;
    }

    public Task ExportMultiSheetAsync<TDto>(TDto data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        var definition =
            _multiSheetRegistry.GetDefinition(
                typeof(TDto));

        ExcelWorkbookData workbook =
            definition.Build(data);

        if (workbook == null)
            throw new InvalidOperationException(
                $"Definition для DTO {typeof(TDto).Name} " +
                "вернула null workbook.");

        string filePath =
            _exportPathProvider.GetExportPath(
                definition.FileName);

        filePath = GetUniqueFilePath(filePath);

        MiniExcel.SaveAs(
            filePath,
            workbook.Sheets);

        Debug.Log(
            $"Multi-sheet Excel exported: {filePath}");

        return Task.CompletedTask;
    }

    private string GetUniqueFilePath(string filePath)
    {
        if (!File.Exists(filePath))
            return filePath;

        string directory =
            Path.GetDirectoryName(filePath);

        string fileNameWithoutExtension =
            Path.GetFileNameWithoutExtension(filePath);

        string extension =
            Path.GetExtension(filePath);

        int counter = 1;

        do
        {
            string newFileName =
                $"{fileNameWithoutExtension}({counter}){extension}";

            filePath = Path.Combine(
                directory,
                newFileName);

            counter++;

        } while (File.Exists(filePath));

        return filePath;
    }
}