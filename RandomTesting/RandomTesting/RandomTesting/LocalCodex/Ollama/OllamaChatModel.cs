// OllamaChatModel.cs
using LocalCodexAgent;
using System.Net.Http.Json;
using System.Text.Json;

namespace RandomTesting.LocalCodex.Ollama;

public sealed class OllamaChatModel : IChatModel
{
    private readonly HttpClient httpClient;
    private readonly Uri baseUri;
    private readonly string modelName;

    public OllamaChatModel(Uri baseUri, string modelName)
    {
        this.httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromHours(1)
        };
        this.baseUri = baseUri;
        this.modelName = modelName;
    }

    public async ValueTask DisposeAsync() => await Task.Run(httpClient.Dispose);

    public async Task<string> GetReplyAsync(IReadOnlyList<AgentMessage> messages, CancellationToken cancellationToken)
    {
        var payload = new
        {
            model = modelName,
            stream = false,
            messages = messages.Select(message => new
            {
                role = message.Role,
                content = message.Content,
                name = message.Name
            }).ToArray()
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(baseUri, "api/chat"));
        request.Content = JsonContent.Create(payload);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);

        if (document.RootElement.TryGetProperty("message", out var messageElement) &&
            messageElement.TryGetProperty("content", out var contentElement))
        {
            return contentElement.GetString() ?? string.Empty;
        }

        if (document.RootElement.TryGetProperty("response", out var responseElement))
        {
            return responseElement.GetString() ?? string.Empty;
        }

        return string.Empty;
    }
}
