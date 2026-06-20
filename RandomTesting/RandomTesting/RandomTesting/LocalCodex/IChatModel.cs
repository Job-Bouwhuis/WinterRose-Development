// IChatModel.cs
namespace LocalCodexAgent;

public interface IChatModel
{
    Task<string> GetReplyAsync(IReadOnlyList<AgentMessage> messages, CancellationToken cancellationToken);
}
