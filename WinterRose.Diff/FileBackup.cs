using System.Security.Cryptography;
using System.Text;

namespace WinterRose.Diff;

internal class FileBackup : IDisposable
{
    private readonly string originalFilePath;
    private readonly string backupPath;

    public FileBackup(string basePath, string originalFilePath)
    {
        this.originalFilePath = originalFilePath;
        
        string hash = GeneratePathHash(originalFilePath);

        backupPath = Path.Combine(basePath, $"{hash}.bak");
    }

    public void EnsureCopy()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);
        File.Copy(originalFilePath, backupPath, overwrite: true);
    }

    public void Restore()
    {
        if (!File.Exists(backupPath))
            throw new FileNotFoundException("Backup does not exist", backupPath);

        File.Copy(backupPath, originalFilePath, overwrite: true);
    }

    internal static string GeneratePathHash(string appName)
    {
        string normalized = appName.Trim().ToLowerInvariant();

        byte[] bytes = SHA1.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public void Dispose()
    {
        if (File.Exists(backupPath))
            File.Delete(backupPath);
    }
}