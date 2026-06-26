using System.Net.Http.Json;
using System.Text.Json;

namespace LocalCodexAgent;

public sealed record OllamaModelEntry(string Name, bool IsInstalled, bool IsRunning);

public sealed class OllamaModelManager
{
    private readonly HttpClient httpClient;
    private readonly Uri baseUri;

    public OllamaModelManager(Uri baseUri)
    {
        this.httpClient = new HttpClient(){
            Timeout = TimeSpan.FromMinutes(5)
        };
        this.baseUri = baseUri;
    }

    public async Task<IReadOnlyList<OllamaModelEntry>> GetModelEntriesAsync(CancellationToken cancellationToken)
    {
        var installedModels = await GetInstalledModelNamesAsync(cancellationToken);
        var runningModels = await GetRunningModelNamesAsync(cancellationToken);

        var allNames = installedModels
            .Concat(runningModels)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return allNames
            .Select(name => new OllamaModelEntry(
                name,
                installedModels.Contains(name, StringComparer.OrdinalIgnoreCase),
                runningModels.Contains(name, StringComparer.OrdinalIgnoreCase)))
            .ToList();
    }

    public async Task EnsureModelAvailableAsync(string modelName, CancellationToken cancellationToken)
    {
        var installedModels = await GetInstalledModelNamesAsync(cancellationToken);

        if (installedModels.Contains(modelName, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        await PullModelAsync(modelName, cancellationToken);
    }

    public async Task StopOtherRunningModelsAsync(string selectedModelName, CancellationToken cancellationToken)
    {
        var runningModels = await GetRunningModelNamesAsync(cancellationToken);

        foreach (var runningModel in runningModels)
        {
            if (string.Equals(runningModel, selectedModelName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            await UnloadModelAsync(runningModel, cancellationToken);
        }
    }

    private async Task<IReadOnlyList<string>> GetInstalledModelNamesAsync(CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(new Uri(baseUri, "api/tags"), cancellationToken);
        response.EnsureSuccessStatusCode();

        return await ReadModelNamesAsync(response, cancellationToken);
    }

    private async Task<IReadOnlyList<string>> GetRunningModelNamesAsync(CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(new Uri(baseUri, "api/ps"), cancellationToken);
        response.EnsureSuccessStatusCode();

        return await ReadModelNamesAsync(response, cancellationToken);
    }

    private async Task PullModelAsync(string modelName, CancellationToken cancellationToken)
    {
        var payload = new
        {
            model = modelName,
            stream = false
        };

        using var response = await httpClient.PostAsJsonAsync(new Uri(baseUri, "api/pull"), payload, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private async Task UnloadModelAsync(string modelName, CancellationToken cancellationToken)
    {
        var payload = new
        {
            model = modelName,
            prompt = string.Empty,
            keep_alive = 0,
            stream = false
        };

        using var response = await httpClient.PostAsJsonAsync(new Uri(baseUri, "api/generate"), payload, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private static async Task<IReadOnlyList<string>> ReadModelNamesAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);

        JsonElement modelsElement = document.RootElement;

        if (document.RootElement.ValueKind == JsonValueKind.Object &&
            document.RootElement.TryGetProperty("models", out var namedModelsElement))
        {
            modelsElement = namedModelsElement;
        }

        if (modelsElement.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<string>();
        }

        var names = new List<string>();

        foreach (var modelElement in modelsElement.EnumerateArray())
        {
            if (TryReadString(modelElement, "name", out var name) || TryReadString(modelElement, "model", out name))
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    names.Add(name);
                }
            }
        }

        return names.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static bool TryReadString(JsonElement element, string propertyName, out string value)
    {
        value = string.Empty;

        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        if (property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = property.GetString() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(value);
    }
}