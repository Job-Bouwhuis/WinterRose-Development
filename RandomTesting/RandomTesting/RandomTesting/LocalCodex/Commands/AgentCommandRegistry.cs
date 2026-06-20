// AgentCommandRegistry.cs
namespace RandomTesting.LocalCodex.Commands;

public sealed class AgentCommandRegistry
{
    private readonly Dictionary<string, IAgentCommand> commands = new(StringComparer.OrdinalIgnoreCase);

    public void Register(IAgentCommand command)
    {
        commands[command.Name] = command;
    }

    public bool TryGet(string name, out IAgentCommand? command)
    {
        if (commands.TryGetValue(name, out command))
        {
            if (CanExecute(command))
                return true;
            throw new InvalidOperationException($"Command '{name}' cannot be executed because we are in read-only mode, and the command is not");
        }
        command = null;
        return false;
    }

    public IReadOnlyCollection<string> AllToolNames => commands.Keys.ToArray();
    public IReadOnlyCollection<IAgentCommand> All => commands.Values.ToArray();

    public bool AskMode { get; set; }

    public bool CanExecute(IAgentCommand command)
    {
        // If ASK mode is enabled, block readonly tools (where IsReadonly is true)
        if (AskMode && !command.IsReadonly)
        {
            return false;
        }
        return true;
    }
}
