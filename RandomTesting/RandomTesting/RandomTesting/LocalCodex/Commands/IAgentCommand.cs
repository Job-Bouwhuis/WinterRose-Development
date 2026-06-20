// IAgentCommand.cs
namespace RandomTesting.LocalCodex.Commands;

public interface IAgentCommand
{
    string Name { get; }
    string Description { get; }
    Task<string> ExecuteAsync(AgentCommandContext context, IReadOnlyDictionary<string, string> arguments, string thought, CancellationToken cancellationToken);
    string GetToolExample();
    bool IsReadonly { get; }
}
