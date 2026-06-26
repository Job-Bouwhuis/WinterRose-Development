using RandomTesting.WebsitePreviews;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace RandomTesting.LocalCodex.Commands;

public sealed class ReadWebPageChunkCommand : IAgentCommand
{
    public string Name => "read_web_page_chunk";
    public string Description => "Fetches readable webpage content and returns it in chunks.";
    public bool IsReadonly => true;

    private static readonly HttpClient CLIENT = new();
    private const int DEFAULT_CHUNK_SIZE = 4000;

    public async Task<string> ExecuteAsync(
        AgentCommandContext context,
        IReadOnlyDictionary<string, string> arguments,
        string thought,
        CancellationToken cancellationToken)
    {
        var url = GetRequired(arguments, "url");

        var chunkIndexRaw = GetOptional(arguments, "chunk");
        var chunkIndex = 1;

        if (!string.IsNullOrWhiteSpace(chunkIndexRaw) && !int.TryParse(chunkIndexRaw, out chunkIndex))
        {
            chunkIndex = 1;
        }

        if (chunkIndex < 1)
        {
            chunkIndex = 1;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            return FormatError($"Invalid URL: {url}");
        }

        string html;

        try
        {
            html = await CLIENT.GetStringAsync(url, cancellationToken);
        }
        catch (Exception ex)
        {
            return FormatError($"Fetch failed: {ex.Message}");
        }

        var text = HtmlTextExtractor.Extract(html);

        if (string.IsNullOrWhiteSpace(text))
        {
            return FormatError("No readable content found");
        }

        var chunks = SplitIntoChunks(text, DEFAULT_CHUNK_SIZE);

        if (chunkIndex > chunks.Count)
        {
            return FormatError($"Chunk out of range. Total chunks: {chunks.Count}");
        }

        var selectedChunk = chunks[chunkIndex - 1];

        return
            "WEB_PAGE_CHUNK:" + Environment.NewLine +
            $"url={url}" + Environment.NewLine +
            $"chunk={chunkIndex}/{chunks.Count}" + Environment.NewLine +
            $"content={selectedChunk}";
    }

    private static List<string> SplitIntoChunks(string text, int size)
    {
        var result = new List<string>();

        for (int i = 0; i < text.Length; i += size)
        {
            var length = Math.Min(size, text.Length - i);
            result.Add(text.Substring(i, length));
        }

        return result;
    }

    private static string GetRequired(IReadOnlyDictionary<string, string> arguments, string name)
    {
        if (!arguments.TryGetValue(name, out var value) || string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Missing required argument: {name}");
        }

        return value.Trim();
    }

    private static string GetOptional(IReadOnlyDictionary<string, string> arguments, string name)
    {
        return arguments.TryGetValue(name, out var value) ? value.Trim() : string.Empty;
    }

    private static string FormatError(string message)
    {
        return "WEB_PAGE_CHUNK_ERROR:" + Environment.NewLine +
               $"success=false" + Environment.NewLine +
               $"message={message}";
    }

    public string GetToolExample()
    {
        return
    @"Tool: read_web_page_chunk

Arguments:
- url: string (required, absolute URL of the webpage to read)
- chunk: int (optional, default = 1; 1-based index of content chunk to retrieve)

Notes:
- Fetches the full HTML content of a webpage and extracts readable text.
- Removes scripts, styles, and raw HTML tags before processing.
- Splits the extracted text into fixed-size chunks for incremental reading.
- Each chunk represents a sequential portion of the same page content.
- Useful for large pages that exceed model context limits.
- Allows the agent to progressively read long documents instead of receiving everything at once.

Failure points:
- This tool is currently non functional and may return nothing or fail with an error.
- Missing required argument: url
- Invalid URL format (must be absolute URI)
- Network failure or timeout during fetch
- Website blocks automated requests or returns non-HTML content
- Extraction yields no readable text
- Requested chunk index exceeds available chunk count
- Server errors (4xx/5xx responses)

Output format:
- Returns WEB_PAGE_CHUNK block containing:
  - url (original page URL)
  - chunk (current chunk index / total chunks)
  - content (text segment for that chunk)";
    }
}

public static class HtmlTextExtractor
{
    public static string Extract(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        // remove scripts/styles
        html = Regex.Replace(html, "<script[\\s\\S]*?</script>", "", RegexOptions.IgnoreCase);
        html = Regex.Replace(html, "<style[\\s\\S]*?</style>", "", RegexOptions.IgnoreCase);

        // remove tags
        html = Regex.Replace(html, "<.*?>", " ");

        // normalize whitespace
        html = Regex.Replace(html, "\\s+", " ").Trim();

        return html;
    }
}