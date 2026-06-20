using LocalCodexAgent;
using RandomTesting.LocalCodex.Commands;
using RandomTesting.WebsitePreviewFetcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

var options = new AgentOptions
{
    WorkspaceRoot = Directory.GetCurrentDirectory(),
    ModelName = "qwen3-coder:30b",
    OllamaBaseUri = new Uri("http://localhost:11434/"),
    MaxIterations = 1000,
    InstructionText = AgentDefaults.InstructionText
};

using var httpClient = new HttpClient();
var model = new OllamaChatModel(httpClient, options.OllamaBaseUri, options.ModelName);
var commandRegistry = DefaultAgentCommands.CreateDefaultRegistry();
var session = new AgentSession(model, options, commandRegistry);

session.AssistantTextReceived += text =>
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write(text);
    Console.ResetColor();
};

session.ToolInvoked += segment =>
{
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.WriteLine($"[{segment.ToolName}]");
    Console.WriteLine(string.Join(Environment.NewLine, segment.Arguments.Select(kvp => $"{kvp.Key}={kvp.Value}")));
    Console.ResetColor();
};

session.ToolOutputReceived += text =>
{
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("[tool]");
    Console.WriteLine(text);
    Console.ResetColor();
};

Console.WriteLine("Local Codex-style agent ready.");
Console.WriteLine("Type a message and press Enter.");
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

    if (string.Equals(input.Trim(), "/exit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    if (string.Equals(input.Trim(), "/read", StringComparison.OrdinalIgnoreCase))
    {
        session.AskMode = true;
        await session.CreateSystemMessage("You are now in read mode. you can not make any changes anymore. you can still read. wait for further instructions.", CancellationToken.None);
        continue;
    }

    if (string.Equals(input.Trim(), "/write", StringComparison.OrdinalIgnoreCase))
    {
        session.AskMode = false; 
        await session.CreateSystemMessage("You are now in write mode. you can make changes aswell as read. wait for further instructions.", CancellationToken.None);
        continue;
    }

    if (input.StartsWith("/setPath", StringComparison.OrdinalIgnoreCase))
    {
        var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

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
            : Path.GetFullPath(Path.Combine(session.WorkingDirectory, rawPath));

        if (!Directory.Exists(fullPath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Directory does not exist: {fullPath}");
            Console.ResetColor();
            continue;
        }

        session.WorkingDirectory = fullPath;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Working directory set to: {fullPath}");
        Console.ResetColor();

        continue;
    }

    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }

    await session.RunAsync(input, CancellationToken.None);
    Console.WriteLine();
}
