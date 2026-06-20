// AgentMessage.cs
namespace LocalCodexAgent;

public sealed record AgentMessage(string Role, string Content, string? Name = null);
