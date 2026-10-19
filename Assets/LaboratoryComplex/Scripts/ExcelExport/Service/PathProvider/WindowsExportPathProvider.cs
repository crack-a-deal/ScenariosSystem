using System;
using System.IO;

public class WindowsExportPathProvider : IExportPathProvider
{
    public string GetExportPath(string fileName)
    {
        string desktopPath = Environment.GetFolderPath(
            Environment.SpecialFolder.Desktop);

        return Path.Combine(
            desktopPath,
            fileName);
    }
}