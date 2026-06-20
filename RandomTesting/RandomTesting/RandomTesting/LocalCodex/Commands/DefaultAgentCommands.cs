// DefaultAgentCommands.cs
using LocalCodexAgent;

namespace RandomTesting.LocalCodex.Commands;

public static class DefaultAgentCommands
{
    public static AgentCommandRegistry CreateDefaultRegistry()
    {
        var registry = new AgentCommandRegistry();
        registry.Register(new ReadFileCommand());
        registry.Register(new ReadRangeCommand());
        registry.Register(new CreateFileCommand());
        //registry.Register(new OverrideLinesCommand());
        //registry.Register(new DeleteLinesCommand());
        registry.Register(new RenameFileCommand());
        registry.Register(new ListFilesCommand());
        registry.Register(new SearchTextCommand());
        registry.Register(new ApplyPatchCommand());
        registry.Register(new WriteAllTextCommand());
        return registry;
    }
}
