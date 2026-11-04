using System.Diagnostics;

namespace WindowsFolderAgent.Services;

public class FolderService
{
    private readonly string[] _allowedRoots =
    {
        @"C:\Archivos",
        @"D:\Archivos",
        @"\\SERVIDOR\Documentos"
    };

    public void OpenFolder(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            throw new ArgumentException("La ruta es obligatoria.");

        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException(
                "Esta operación requiere Windows.");

        var fullPath = Path.GetFullPath(folderPath);

        var allowed = _allowedRoots.Any(root =>
        {
            var normalizedRoot = Path.GetFullPath(root)
                .TrimEnd(Path.DirectorySeparatorChar,
                         Path.AltDirectorySeparatorChar);

            return fullPath.Equals(
                       normalizedRoot,
                       StringComparison.OrdinalIgnoreCase)
                   ||
                   fullPath.StartsWith(
                       normalizedRoot + Path.DirectorySeparatorChar,
                       StringComparison.OrdinalIgnoreCase);
        });

        if (!allowed)
            throw new UnauthorizedAccessException(
                "La ubicación no está autorizada.");

        if (!Directory.Exists(fullPath))
            throw new DirectoryNotFoundException(
                "La carpeta no existe.");

        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"\"{fullPath}\"",
            UseShellExecute = true
        });
    }
}
