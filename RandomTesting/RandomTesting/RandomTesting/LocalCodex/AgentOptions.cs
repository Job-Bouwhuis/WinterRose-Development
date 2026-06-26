// AgentOptions.cs
namespace LocalCodexAgent;

public sealed class AgentOptions
{
    private string workspaceRoot = Directory.GetCurrentDirectory();
    public string WorkspaceRoot
    {
        get => workspaceRoot;
        init => workspaceRoot = value;
    }

    public string ModelName { get; init; } = "qwen3-coder:30b";
    public Uri OllamaBaseUri { get; init; } = new Uri("http://localhost:11434/");
    public int MaxIterations { get; init; } = 8;
    private string instructionText = AgentDefaults.InstructionText;
    /// <summary>
    /// Expected to contain <code>{{AVAILABLE_TOOLS}}</code> as a placeholder for the tool list, which will be injected at runtime.
    /// </summary>
    public string InstructionText 
    { 
        get => instructionText; 
        init => instructionText = value;
    }

    internal void SetWorkspaceRoot(string workspaceRoot)
    {
        this.workspaceRoot = workspaceRoot;
    }

    internal void SetToolList(IEnumerable<string> toolExamples)
    {
        var toolSections = toolExamples.Select(example =>
            "-------\n" +
            example.Trim() +
            "\n-------");

        var toolListText =
            "AVAILABLE TOOLS\n" +
            "================\n" +
            string.Join(Environment.NewLine + Environment.NewLine, toolSections);

        instructionText = InstructionText.Replace("{{AVAILABLE_TOOLS}}", toolListText);
    }
}
