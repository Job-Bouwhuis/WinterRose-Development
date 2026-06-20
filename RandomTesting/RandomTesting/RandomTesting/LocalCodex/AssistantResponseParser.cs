// AssistantResponseParser.cs
using System.Text;

namespace LocalCodexAgent;

public static class AssistantResponseParser
{
    public static IReadOnlyList<ResponseSegment> Parse(string response)
    {
        var normalizedResponse = response.Replace("\r\n", "\n");
        var lines = normalizedResponse.Split('\n');
        var segments = new List<ResponseSegment>();
        var textBuffer = new StringBuilder();
        var inToolBlock = false;
        var currentToolName = string.Empty;
        var toolLines = new List<string>();

        void FlushText()
        {
            if (textBuffer.Length > 0)
            {
                segments.Add(new TextSegment(textBuffer.ToString()));
                textBuffer.Clear();
            }
        }

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (!inToolBlock)
            {
                if (trimmedLine.StartsWith("<<tool:", StringComparison.OrdinalIgnoreCase) && trimmedLine.EndsWith(">>"))
                {
                    FlushText();
                    currentToolName = trimmedLine.Substring("<<tool:".Length, trimmedLine.Length - "<<tool:".Length - 2).Trim();
                    toolLines.Clear();
                    inToolBlock = true;
                    continue;
                }
                if(trimmedLine.StartsWith("<<tool="))
                {
                    FlushText();
                    segments.Add(new ToolSegment("Error", new Dictionary<string, string>
                    {
                        ["message"] = "Invalid tool syntax <<tool=TOOLNAME>>",
                        ["suggestion"] = "Use the correct syntax: <<tool:TOOLNAME>>"
                    }, ""));
                    break;
                }

                textBuffer.AppendLine(line);
                continue;
            }

            if (string.Equals(trimmedLine, "<<end>>", StringComparison.OrdinalIgnoreCase))
            {
                var parsedTool = ParseToolBlock(currentToolName, toolLines);
                segments.Add(parsedTool);
                inToolBlock = false;
                currentToolName = string.Empty;
                toolLines.Clear();
                continue;
            }

            toolLines.Add(line);
        }

        if (inToolBlock)
        {
            textBuffer.AppendLine("<<tool:" + currentToolName + ">>");
            foreach (var line in toolLines)
            {
                textBuffer.AppendLine(line);
            }

            FlushText();
            return segments;
        }

        FlushText();
        return segments;
    }

    private static ToolSegment ParseToolBlock(string toolName, IReadOnlyList<string> lines)
    {
        var arguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var thoughtLines = new List<string>();
        string? currentKey = null;
        var currentValue = new StringBuilder();
        var contentMode = false;

        void FlushCurrent()
        {
            if (!string.IsNullOrWhiteSpace(currentKey))
            {
                arguments[currentKey] = currentValue.ToString();
            }

            currentKey = null;
            currentValue.Clear();
        }

        foreach (var line in lines)
        {
            if (contentMode)
            {
                currentValue.AppendLine(line);
                continue;
            }

            if (TryParseKeyValue(line, out var key, out var value))
            {
                FlushCurrent();
                currentKey = key;
                currentValue.Append(value);

                if (string.Equals(key, "content", StringComparison.OrdinalIgnoreCase))
                {
                    contentMode = true;
                }

                continue;
            }

            if (!string.IsNullOrWhiteSpace(line))
            {
                thoughtLines.Add(line);
            }
        }

        FlushCurrent();

        return new ToolSegment(
            toolName,
            arguments,
            string.Join(Environment.NewLine, thoughtLines));
    }

    private static bool TryParseKeyValue(string line, out string key, out string value)
    {
        var index = line.IndexOf('=');
        if (index <= 0)
        {
            key = string.Empty;
            value = string.Empty;
            return false;
        }

        key = line[..index].Trim();
        value = line[(index + 1)..];
        return key.Length > 0;
    }
}
