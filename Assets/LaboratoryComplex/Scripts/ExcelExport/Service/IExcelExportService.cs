using System.Collections.Generic;
using System.Threading.Tasks;

public interface IExcelExportService
{
    Task ExportAsync<TDto>(
        IEnumerable<TDto> data);

    Task ExportMultiSheetAsync<TDto>(
        TDto data);

    string GetExportPath();
}