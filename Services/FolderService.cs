
using System.Diagnostics;

namespace WindowsFolderAgent.Services;

public class FolderService
{
    private readonly string[] _allowedRoots;

    public FolderService(IConfiguration configuration)
    {
        _allowedRoots = configuration
            .GetSection("AgentSettings:AllowedRoots")
            .Get<string[]>() ?? [];
    }

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

        var authorized = _allowedRoots.Any(root =>
        {
            var normalizedRoot = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(root)
            );

            var relativePath = Path.GetRelativePath(
                normalizedRoot,
                fullPath
            );

            return relativePath == "." ||
                   (!Path.IsPathRooted(relativePath) &&
                    relativePath != ".." &&
                    !relativePath.StartsWith(
                        ".." + Path.DirectorySeparatorChar,
                        StringComparison.Ordinal));
        });

        if (!authorized)
        {
            throw new UnauthorizedAccessException(
                "La carpeta está fuera de las ubicaciones autorizadas.");
        }

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
