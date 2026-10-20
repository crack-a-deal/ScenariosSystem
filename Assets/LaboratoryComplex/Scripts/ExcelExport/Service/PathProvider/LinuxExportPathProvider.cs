using System;
using System.IO;

public class LinuxExportPathProvider : IExportPathProvider
{
    public string GetExportPath(string fileName)
    {
        string homePath = Environment.GetFolderPath(
            Environment.SpecialFolder.Personal);

        string desktopPath = Path.Combine(homePath, "Desktop");

        if (!Directory.Exists(desktopPath))
            Directory.CreateDirectory(desktopPath);

        return Path.Combine(desktopPath, fileName);
    }
}