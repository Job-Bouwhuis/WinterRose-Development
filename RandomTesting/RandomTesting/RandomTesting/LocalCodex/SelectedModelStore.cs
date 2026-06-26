using System.Linq;

namespace RandomTesting.LocalCodex;

public static class SelectedModelStore
{
    private const string FolderName = "LocalCodexAgent";
    private const string FileName = "selected-model.txt";

    public static string Load(IReadOnlyCollection<string> configuredModels, string fallbackModelName)
    {
        var path = GetPath();

        if (File.Exists(path))
        {
            var storedModel = File.ReadAllText(path).Trim();

            if (!string.IsNullOrWhiteSpace(storedModel) &&
                configuredModels.Contains(storedModel, StringComparer.OrdinalIgnoreCase))
            {
                return storedModel;
            }
        }

        if (configuredModels.Contains(fallbackModelName, StringComparer.OrdinalIgnoreCase))
        {
            return fallbackModelName;
        }

        return configuredModels.First();
    }

    public static void Save(string modelName)
    {
        var path = GetPath();
        var folder = Path.GetDirectoryName(path);

        if (!string.IsNullOrWhiteSpace(folder))
        {
            Directory.CreateDirectory(folder);
        }

        File.WriteAllText(path, modelName);
    }

    private static string GetPath()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            FolderName);

        return Path.Combine(folder, FileName);
    }
}