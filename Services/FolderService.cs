using System.Diagnostics;

namespace WindowsFolderAgent.Services;

public class FolderService
{
    public void OpenFolder(string folderPath)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException(
                "Esta operación requiere Windows.");
        }

        if (string.IsNullOrWhiteSpace(folderPath))
        {
            throw new ArgumentException(
                "La ruta de la carpeta es obligatoria.");
        }

        var fullPath = Path.GetFullPath(folderPath);

        if (!Directory.Exists(fullPath))
        {
            throw new DirectoryNotFoundException(
                "La carpeta no existe.");
        }

        var processInfo = new ProcessStartInfo
        {
            FileName = "explorer.exe",
            UseShellExecute = true
        };

        processInfo.ArgumentList.Add(fullPath);

        Process.Start(processInfo);
    }
}
