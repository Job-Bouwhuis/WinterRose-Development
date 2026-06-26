using LocalCodexAgent;
using RandomTesting.LocalCodex;
using RandomTesting.LocalCodex.Commands;
using RandomTesting.LocalCodex.Ollama;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

const string DEFAULT_MODEL_NAME = "qwen3-coder:30b";

var configuredModels = new[]
{
    "qwen3-coder:30b",
    "llama3.1:8b",
    "gemma3:12b"
};

var baseOptions = new AgentOptions
{
    WorkspaceRoot = Directory.GetCurrentDirectory(),
    ModelName = DEFAULT_MODEL_NAME,
    OllamaBaseUri = new Uri("http://localhost:11434/"),
    MaxIterations = 1000,
    InstructionText = AgentDefaults.InstructionText
};

using var ollamaRuntime = new OllamaRuntimeManager(baseOptions.OllamaBaseUri);
var ollamaModels = new OllamaModelManager(baseOptions.OllamaBaseUri);

await ollamaRuntime.EnsureRunningAsync(CancellationToken.None);

var selectedModelName = SelectedModelStore.Load(configuredModels, DEFAULT_MODEL_NAME);
AgentSession? session = await CreateSessionAsync(selectedModelName, baseOptions, ollamaModels, CancellationToken.None);

BindSessionEvents(session);
WriteHeader(selectedModelName);

Console.WriteLine("Local Codex-style agent ready.");
Console.WriteLine("Type a message and press Enter.");
Console.WriteLine("Type /models to list and switch models.");
Console.WriteLine("Type /exit to quit.");
Console.WriteLine();

while (true)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("> ");
    var input = Console.ReadLine();
    Console.ResetColor();

    if (input is null)
    {
        break;
    }

    var command = input.Trim();

    if (string.Equals(command, "/exit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    if (string.Equals(command, "/models", StringComparison.OrdinalIgnoreCase))
    {
        var nextModelName = await ShowModelMenuAsync(
            selectedModelName,
            configuredModels,
            ollamaModels,
            CancellationToken.None);

        if (!string.Equals(nextModelName, selectedModelName, StringComparison.OrdinalIgnoreCase))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Switching models will clear the current context.");
            Console.Write("Continue? [y/N]: ");
            Console.ResetColor();

            var confirmation = Console.ReadLine();

            if (string.Equals(confirmation?.Trim(), "y", StringComparison.OrdinalIgnoreCase))
            {
                selectedModelName = nextModelName;
                SelectedModelStore.Save(selectedModelName);

                await ReplaceSessionAsync(
                    selectedModelName,
                    baseOptions,
                    ollamaModels,
                    CancellationToken.None);
            }
        }

        continue;
    }

    if (string.Equals(command, "/read", StringComparison.OrdinalIgnoreCase))
    {
        session!.AskMode = true;
        await session.CreateSystemMessage("You are now in read mode. you can not make any changes anymore. you can still read. wait for further instructions.", CancellationToken.None);
        continue;
    }

    if (string.Equals(command, "/write", StringComparison.OrdinalIgnoreCase))
    {
        session!.AskMode = false;
        await session.CreateSystemMessage("You are now in write mode. you can make changes aswell as read. wait for further instructions.", CancellationToken.None);
        continue;
    }

    if (string.Equals(command, "/path", StringComparison.OrdinalIgnoreCase))
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Current workspace path: {session!.WorkspaceRoot}");
        Console.ResetColor();
        continue;
    }

    if (command.StartsWith("/setPath", StringComparison.OrdinalIgnoreCase))
    {
        var parts = command.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[1]))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Usage: /setPath <absolute-or-relative-path>");
            Console.ResetColor();
            continue;
        }

        var rawPath = parts[1].Trim();

        var fullPath = Path.IsPathRooted(rawPath)
            ? rawPath
            : Path.GetFullPath(Path.Combine(session!.WorkspaceRoot, rawPath));

        if (!Directory.Exists(fullPath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Directory does not exist: {fullPath}");
            Console.ResetColor();
            continue;
        }

        session!.WorkspaceRoot = fullPath;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Working directory set to: {fullPath}");
        Console.ResetColor();

        continue;
    }

    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }

    await session!.RunAsync(input, CancellationToken.None);
    Console.WriteLine();
}

await session.DisposeAsync();


var shutdownChoice = await ConsoleCountdownPrompt.AskYesNoAsync(
    "Ollama was started by this app. Shut it down too?",
    TimeSpan.FromSeconds(5),
    CancellationToken.None);

if (shutdownChoice == true)
{
    await ollamaRuntime.RequestShutdownAsync(CancellationToken.None);
}

async Task ReplaceSessionAsync(
    string modelName,
    AgentOptions templateOptions,
    OllamaModelManager modelManager,
    CancellationToken cancellationToken)
{
    var newSession = await CreateSessionAsync(modelName, templateOptions, modelManager, cancellationToken);

    await session.DisposeAsync();


    session = newSession;
    BindSessionEvents(session);
    Console.Clear();
    WriteHeader(modelName);
}

async Task<AgentSession> CreateSessionAsync(
    string modelName,
    AgentOptions templateOptions,
    OllamaModelManager modelManager,
    CancellationToken cancellationToken)
{
    await modelManager.EnsureModelAvailableAsync(modelName, cancellationToken);
    await modelManager.StopOtherRunningModelsAsync(modelName, cancellationToken);

    var sessionOptions = CreateOptions(templateOptions, modelName);
    var model = new OllamaChatModel(sessionOptions.OllamaBaseUri, sessionOptions.ModelName);
    var commandRegistry = DefaultAgentCommands.CreateDefaultRegistry();

    return new AgentSession(model, sessionOptions, commandRegistry);
}

AgentOptions CreateOptions(AgentOptions templateOptions, string modelName)
{
    return new AgentOptions
    {
        WorkspaceRoot = templateOptions.WorkspaceRoot,
        ModelName = modelName,
        OllamaBaseUri = templateOptions.OllamaBaseUri,
        MaxIterations = templateOptions.MaxIterations,
        InstructionText = templateOptions.InstructionText
    };
}

void BindSessionEvents(AgentSession currentSession)
{
    currentSession.AssistantTextReceived += text =>
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write(text);
        Console.ResetColor();
    };

    currentSession.ToolInvoked += segment =>
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"[{segment.ToolName}]");
        Console.WriteLine(string.Join(Environment.NewLine, segment.Arguments.Select(kvp => $"{kvp.Key}={kvp.Value}")));
        Console.ResetColor();
    };

    currentSession.ToolOutputReceived += text =>
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("[tool]");
        Console.WriteLine(text);
        Console.ResetColor();
    };
}

void WriteHeader(string modelName)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"Current model: {modelName}");
    Console.ResetColor();
    Console.WriteLine();
}

async Task<string> ShowModelMenuAsync(
    string currentModelName,
    IReadOnlyList<string> modelNames,
    OllamaModelManager modelManager,
    CancellationToken cancellationToken)
{
    var modelEntries = await modelManager.GetModelEntriesAsync(cancellationToken);

    Console.WriteLine();
    Console.WriteLine("Available models:");
    Console.WriteLine();

    for (var index = 0; index < modelNames.Count; index++)
    {
        var modelName = modelNames[index];
        var entry = modelEntries.FirstOrDefault(item => string.Equals(item.Name, modelName, StringComparison.OrdinalIgnoreCase));

        var statusParts = new List<string>();

        if (entry is not null)
        {
            if (entry.IsInstalled)
            {
                statusParts.Add("installed");
            }
            else
            {
                statusParts.Add("will pull");
            }

            if (entry.IsRunning)
            {
                statusParts.Add("running");
            }
        }
        else
        {
            statusParts.Add("will pull");
        }

        var currentTag = string.Equals(modelName, currentModelName, StringComparison.OrdinalIgnoreCase)
            ? " (current)"
            : string.Empty;

        Console.WriteLine($"{index + 1}. {modelName}{currentTag} [{string.Join(", ", statusParts)}]");
    }

    Console.WriteLine();
    Console.WriteLine("C. Custom model name");
    Console.Write("Select a model: ");

    var choice = Console.ReadLine()?.Trim();

    if (string.Equals(choice, "c", StringComparison.OrdinalIgnoreCase))
    {
        Console.Write("Enter model name: ");
        var customModelName = Console.ReadLine()?.Trim();

        if (!string.IsNullOrWhiteSpace(customModelName))
        {
            return customModelName;
        }

        return currentModelName;
    }

    if (int.TryParse(choice, out var selectedIndex) &&
        selectedIndex >= 1 &&
        selectedIndex <= modelNames.Count)
    {
        return modelNames[selectedIndex - 1];
    }

    return currentModelName;
}