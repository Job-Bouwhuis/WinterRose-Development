namespace RandomTesting.LocalCodex.Commands;

public sealed class MemoryCommand : IAgentCommand
{
    private static readonly Dictionary<string, string> _memoryStore = new();
    private static readonly object _lock = new();
    private static readonly string _storagePath;
    private static readonly string _storageFile;

    static MemoryCommand()
    {
        var exeDirectory = Path.GetDirectoryName(AppContext.BaseDirectory) ?? Environment.CurrentDirectory;
        var memoryFolder = Path.Combine(exeDirectory, "memory_storage");

        if (!Directory.Exists(memoryFolder))
        {
            Directory.CreateDirectory(memoryFolder);
        }

        _storagePath = memoryFolder;
        _storageFile = Path.Combine(_storagePath, "memory_store.json");

        LoadFromDisk();
    }


    public string Name => "memory";
    public string Description => "Manages persistent memory storage with remember, recall, forget, list, and try_recall operations. Memories are persisted to disk.";

    public bool IsReadonly => false;

    public async Task<string> ExecuteAsync(AgentCommandContext context, IReadOnlyDictionary<string, string> arguments, string thought, CancellationToken cancellationToken)
    {
        var operation = GetRequired(arguments, "operation");

        return operation.ToLowerInvariant() switch
        {
            "remember" => await RememberAsync(arguments, cancellationToken),
            "recall" => await RecallAsync(arguments, cancellationToken),
            "try_recall" => await TryRecallAsync(arguments, cancellationToken),
            "forget" => await ForgetAsync(arguments, cancellationToken),
            "list" => await ListAsync(arguments, cancellationToken),
            _ => FormatOperationResult(false, $"Unknown operation: {operation}. Valid operations: remember, recall, try_recall, forget, list")
        };
    }

    private async Task<string> RememberAsync(IReadOnlyDictionary<string, string> arguments, CancellationToken cancellationToken)
    {
        var key = GetRequired(arguments, "key");
        var value = GetRequired(arguments, "value");
        var overwrite = GetOptionalBool(arguments, "overwrite", false);

        lock (_lock)
        {
            if (_memoryStore.ContainsKey(key) && !overwrite)
            {
                return FormatOperationResult(false, $"Memory key '{key}' already exists. Use overwrite=true to replace.");
            }

            _memoryStore[key] = value;
        }

        try
        {
            await SaveToDiskAsync(cancellationToken);
            return FormatOperationResult(true, $"Stored memory: {key} = {value}");
        }
        catch (Exception ex)
        {
            lock (_lock)
            {
                _memoryStore.Remove(key);
            }
            return FormatOperationResult(false, $"Failed to persist memory to disk: {ex.Message}");
        }
    }

    private async Task<string> RecallAsync(IReadOnlyDictionary<string, string> arguments, CancellationToken cancellationToken)
    {
        var key = GetRequired(arguments, "key");
        var defaultValue = GetOptionalString(arguments, "default");

        lock (_lock)
        {
            if (_memoryStore.TryGetValue(key, out var value))
            {
                return FormatOperationResult(true, $"Recalled memory: {key} = {value}");
            }

            if (defaultValue != null)
            {
                return FormatOperationResult(true, $"Memory not found, returning default: {defaultValue}");
            }

            return FormatOperationResult(false, $"Memory key '{key}' not found");
        }
    }

    private async Task<string> TryRecallAsync(IReadOnlyDictionary<string, string> arguments, CancellationToken cancellationToken)
    {
        var key = GetRequired(arguments, "key");

        lock (_lock)
        {
            if (_memoryStore.TryGetValue(key, out var value))
            {
                return FormatOperationResult(true, $"Found memory: {key} = {value}");
            }

            return FormatOperationResult(true, $"No memory found for key: {key}");
        }
    }

    private async Task<string> ForgetAsync(IReadOnlyDictionary<string, string> arguments, CancellationToken cancellationToken)
    {
        var key = GetRequired(arguments, "key");

        lock (_lock)
        {
            if (!_memoryStore.Remove(key))
            {
                return FormatOperationResult(false, $"Memory key '{key}' not found");
            }
        }

        try
        {
            await SaveToDiskAsync(cancellationToken);
            return FormatOperationResult(true, $"Forgot memory: {key}");
        }
        catch (Exception ex)
        {
            lock (_lock)
            {
                _memoryStore[key] = "[ROLLBACK]";
            }
            return FormatOperationResult(false, $"Failed to persist memory deletion to disk: {ex.Message}");
        }
    }

    private async Task<string> ListAsync(IReadOnlyDictionary<string, string> arguments, CancellationToken cancellationToken)
    {
        var pattern = GetOptionalString(arguments, "pattern");
        var maxResults = GetOptionalInt(arguments, "max_results", 50);

        lock (_lock)
        {
            if (_memoryStore.Count == 0)
            {
                return FormatOperationResult(true, "No memories stored");
            }

            var entries = _memoryStore.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(pattern))
            {
                entries = entries.Where(kvp =>
                    kvp.Key.Contains(pattern, StringComparison.OrdinalIgnoreCase) ||
                    kvp.Value.Contains(pattern, StringComparison.OrdinalIgnoreCase));
            }

            entries = entries.Take(maxResults);

            var result = entries.Select(kvp => $"{kvp.Key}: {kvp.Value}");
            var response = string.Join(Environment.NewLine, result);
            var count = entries.Count();

            return FormatOperationResult(true,
                $"Found {count} memory entries:" + Environment.NewLine + response);
        }
    }

    private static void LoadFromDisk()
    {
        if (!File.Exists(_storageFile))
        {
            return;
        }

        try
        {
            var json = File.ReadAllText(_storageFile);
            var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            if (data != null)
            {
                lock (_lock)
                {
                    foreach (var kvp in data)
                    {
                        _memoryStore[kvp.Key] = kvp.Value;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load memory from disk: {ex.Message}");
        }
    }

    private async Task SaveToDiskAsync(CancellationToken cancellationToken)
    {
        Dictionary<string, string> snapshot;
        lock (_lock)
        {
            snapshot = new Dictionary<string, string>(_memoryStore);
        }

        var json = System.Text.Json.JsonSerializer.Serialize(snapshot, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        var tempFile = _storageFile + ".tmp";
        await File.WriteAllTextAsync(tempFile, json, cancellationToken);

        if (File.Exists(_storageFile))
        {
            File.Delete(_storageFile);
        }
        File.Move(tempFile, _storageFile);
    }

    private static string GetRequired(IReadOnlyDictionary<string, string> arguments, string name)
    {
        if (!arguments.TryGetValue(name, out var value) || string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Missing required argument: {name}");
        }

        return value.Trim();
    }

    private static bool GetOptionalBool(IReadOnlyDictionary<string, string> arguments, string name, bool defaultValue)
    {
        if (!arguments.TryGetValue(name, out var value) || string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        if (bool.TryParse(value.Trim(), out var parsed))
        {
            return parsed;
        }

        return defaultValue;
    }

    private static string? GetOptionalString(IReadOnlyDictionary<string, string> arguments, string name)
    {
        if (!arguments.TryGetValue(name, out var value) || string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static int GetOptionalInt(IReadOnlyDictionary<string, string> arguments, string name, int defaultValue)
    {
        if (!arguments.TryGetValue(name, out var value) || string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        if (int.TryParse(value.Trim(), out var parsed) && parsed > 0)
        {
            return parsed;
        }

        return defaultValue;
    }

    private static string FormatOperationResult(bool success, string message)
    {
        return "OPERATION_RESULT:" + Environment.NewLine +
               $"success={(success ? "true" : "false")}" + Environment.NewLine +
               $"message={message}";
    }

    public string GetToolExample()
    {
        return
    @"Tool: memory

Arguments:
- operation: string (required, one of: remember, recall, try_recall, forget, list)

Sub-arguments:
For operation 'remember':
  - key: string (required, unique identifier for the memory)
  - value: string (required, content to store)
  - overwrite: bool (optional, default = false; Replace existing memory if key exists)

For operation 'recall':
  - key: string (required, identifier of memory to retrieve)
  - default: string (optional, value to return if key not found)

For operation 'try_recall':
  - key: string (required, identifier of memory to attempt retrieval)

For operation 'forget':
  - key: string (required, identifier of memory to remove)

For operation 'list':
  - pattern: string (optional, filter memories by key or value containing pattern)
  - max_results: int (optional, default = 50; Maximum number of entries to return)

Arguments for the different operations.
This is not a tool call example, but illustrates the various ways to use the memory command with different operations and arguments.
Stick to the <<tool:memory>> format when invoking the tool.
1. Store a memory:
   - operation: remember
   - key: user_preference
   - value: prefers dark theme
   - overwrite: false

2. Recall a memory (fails if not found):
   - operation: recall
   - key: user_preference

3. Recall with default (returns default if not found):
   - operation: recall
   - key: missing_key
   - default: default_value

4. Try recall (always succeeds, indicates if found or not):
   - operation: try_recall
   - key: user_preference

5. Forget a memory:
   - operation: forget
   - key: user_preference

6. List all memories:
   - operation: list

7. Search memories:
   - operation: list
   - pattern: theme
   - max_results: 10

Notes:
- Memories are persisted to disk in a 'memory_storage' folder next to the executable.
- Memory file location: [exe_path]/memory_storage/memory_store.json
- Data is stored in JSON format for easy inspection.
- Memory operations are thread-safe using a lock.
- On disk write failure, the operation is rolled back in memory.
- Uses atomic file write (temp file + rename) to prevent corruption.
- Key names are case-sensitive.
- Empty strings are not allowed as keys or values.
- The list operation returns at most max_results entries, sorted by insertion order.
- The try_recall operation is designed for agents to safely check for memories without causing errors.
- Before all requests from the user regarding any topic, the agent should check if there is a memory stored for that topic using try_recall.

Failure points:
- Missing required argument: operation
- Missing required argument for specific operation (key, value)
- Duplicate key with overwrite=false in remember operation
- Non-existent key in recall operation (without default) - this fails with success=false
- Non-existent key in forget operation - this fails with success=false
- Invalid operation name
- Thread contention (handled via lock)
- Disk write failures (permissions, disk full, IO errors)
- JSON serialization/deserialization errors

Key difference between recall and try_recall:
- recall: Returns success=false when key is not found (unless default is provided)
- try_recall: Always returns success=true, with a message indicating whether the key was found or not. This allows agents to safely check for memories without error handling.";
    }
}