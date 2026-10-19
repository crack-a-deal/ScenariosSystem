using System.Collections.Generic;

public class ExcelWorkbookData
{
    public Dictionary<string, object> Sheets { get; }

    public ExcelWorkbookData()
    {
        Sheets = new Dictionary<string, object>();
    }
}