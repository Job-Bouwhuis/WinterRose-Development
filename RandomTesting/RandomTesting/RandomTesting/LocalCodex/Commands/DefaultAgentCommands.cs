namespace RandomTesting.LocalCodex.Commands;

public static class DefaultAgentCommands
{
    public static AgentCommandRegistry CreateDefaultRegistry()
    {
        var registry = new AgentCommandRegistry();
        registry.Register(new ReadFileCommand());
        registry.Register(new ReadRangeCommand());
        registry.Register(new CreateFileCommand());
        registry.Register(new RenameFileCommand());
        registry.Register(new DeleteFileCommand());
        registry.Register(new ListFilesCommand());
        registry.Register(new SearchTextCommand());
        registry.Register(new ApplyPatchCommand());
        registry.Register(new WriteAllTextCommand());
        registry.Register(new OpenExplorerCommand());
        registry.Register(new WebPagePreviewCommand());
        registry.Register(new ReadWebPageChunkCommand());
        registry.Register(new GoogleSearchCommand());
        registry.Register(new MemoryCommand());
        registry.Register(new ExecuteCSharpCodeCommand());
        return registry;
    }
}