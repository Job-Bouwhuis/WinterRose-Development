// IChatModel.cs
namespace LocalCodexAgent;

public interface IChatModel : IAsyncDisposable
{
    Task<string> GetReplyAsync(IReadOnlyList<AgentMessage> messages, CancellationToken cancellationToken);
}
