// LocalCodexAgent.cs
using RandomTesting.LocalCodex.Commands;
using System.Linq;
using System.Net.Http.Headers;
using WinterRose.FuzzySearching;

namespace LocalCodexAgent;

public sealed class AgentSession : IAsyncDisposable
{
    private readonly IChatModel chatModel;
    private readonly AgentOptions options;
    private readonly AgentCommandRegistry commandRegistry;
    private readonly AgentCommandContext commandContext;
    private readonly List<AgentMessage> messages = new();

    public string WorkspaceRoot
    {
        get => options.WorkspaceRoot;
        set
        {
            commandContext.WorkspaceRoot = value;
            options.SetWorkspaceRoot(value);
        }
    }

    public bool AskMode
    {
        get => commandRegistry.AskMode;
        set => commandRegistry.AskMode = value;
    }

    public event Action<string>? AssistantTextReceived;
    public event Action<ToolSegment> ToolInvoked;
    public event Action<string>? ToolOutputReceived;

    public AgentSession(IChatModel chatModel, AgentOptions options, AgentCommandRegistry commandRegistry)
    {
        this.chatModel = chatModel;
        this.options = options;
        this.commandRegistry = commandRegistry;
        commandContext = new AgentCommandContext(options.WorkspaceRoot);

        options.SetToolList(commandRegistry.All.Select(command => command.GetToolExample()));

        messages.Add(new AgentMessage("system", options.InstructionText));
    }

    public async Task RunAsync(string userMessage, CancellationToken cancellationToken)
    {
        messages.Add(new AgentMessage("user", userMessage));

        for (var iteration = 0; iteration < options.MaxIterations; iteration++)
        {
            var assistantResponse = await chatModel.GetReplyAsync(messages, cancellationToken);
            messages.Add(new AgentMessage("assistant", assistantResponse));

            var segments = AssistantResponseParser.Parse(assistantResponse);
            var usedTool = false;

            foreach (var segment in segments)
            {
                if (segment is TextSegment textSegment)
                {
                    if (!string.IsNullOrWhiteSpace(textSegment.Text))
                    {
                        AssistantTextReceived?.Invoke(textSegment.Text);
                    }

                    continue;
                }

                if (segment is ToolSegment toolSegment)
                {
                    ToolInvoked?.Invoke(toolSegment);
                    usedTool = true;
                    var toolResult = await ExecuteToolAsync(toolSegment, cancellationToken);
                    ToolOutputReceived?.Invoke(toolResult);
                    messages.Add(new AgentMessage("tool", toolResult, toolSegment.ToolName));
                }
            }

            if (!usedTool)
            {
                return;
            }
        }

        AssistantTextReceived?.Invoke(Environment.NewLine + "[agent] Maximum iteration count reached." + Environment.NewLine);
    }

    private async Task<string> ExecuteToolAsync(ToolSegment toolSegment, CancellationToken cancellationToken)
    {
        try
        {
            if (!commandRegistry.TryGet(toolSegment.ToolName, out var command) || command is null)
            {
                string likelyMatch = commandRegistry.AllToolNames.Search(toolSegment.ToolName);

                return "OPERATION_RESULT:" + Environment.NewLine +
                       "success=false" + Environment.NewLine +
                       $"message=Unknown tool: {toolSegment.ToolName}" + Environment.NewLine +
                       $"suggestion=The most closest toolname match for {toolSegment.ToolName} is {likelyMatch}";
            }

            return await command.ExecuteAsync(commandContext, toolSegment.Arguments, toolSegment.Thought, cancellationToken);
        }
        catch (Exception exception)
        {
            return "OPERATION_RESULT:" + Environment.NewLine +
                   "success=false" + Environment.NewLine +
                   $"message={exception.Message}";
        }
    }

    public async Task CreateSystemMessage(string v, CancellationToken none)
    {
        messages.Add(new AgentMessage("system", v));
    }

    public async ValueTask DisposeAsync()
    {
        await chatModel.DisposeAsync();
    }
}