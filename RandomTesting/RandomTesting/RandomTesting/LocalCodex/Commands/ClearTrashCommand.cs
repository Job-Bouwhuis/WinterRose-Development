using System;
using System.Collections.Generic;
using System.Text;

namespace RandomTesting.LocalCodex.Commands;

internal class ClearTrashCommand : IAgentCommand
{
    public string Name => "clear_trash";

    public string Description => "Clears the trash.";

    public bool IsReadonly => false;

    public Task<string> ExecuteAsync(AgentCommandContext context, IReadOnlyDictionary<string, string> arguments, string thought, CancellationToken cancellationToken)
    {
        var exeDirectory = AppContext.BaseDirectory;
        var trashRoot = Path.Combine(exeDirectory, "trash");

        Directory.Delete(trashRoot, true);
        return Task.FromResult("");
    }

    public string GetToolExample() => "Tool: \"clear_trash\" - Not to be invoked without explicit user consent. Clears the trash directory where deleted files are stored.";
}
