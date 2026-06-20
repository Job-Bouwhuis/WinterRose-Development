// ToolSegment.cs
namespace LocalCodexAgent;

public sealed record ToolSegment(string ToolName, IReadOnlyDictionary<string, string> Arguments, string Thought) : ResponseSegment;
