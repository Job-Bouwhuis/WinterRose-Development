using RandomTesting.WebsitePreviews;

namespace RandomTesting.LocalCodex.Commands;

public sealed class WebPagePreviewCommand : IAgentCommand
{
    public string Name => "read_web_preview";
    public string Description => "Fetches structured metadata preview from a webpage (title, description, image, etc).";
    public bool IsReadonly => false;

    private static readonly WebsitePreviewFetcher FETCHER = new();

    public async Task<string> ExecuteAsync(
        AgentCommandContext context,
        IReadOnlyDictionary<string, string> arguments,
        string thought,
        CancellationToken cancellationToken)
    {
        var url = GetRequired(arguments, "url");

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            return FormatError($"Invalid URL: {url}");
        }

        WebsitePreview preview;

        try
        {
            preview = await FETCHER.FetchPreviewAsync(url);
        }
        catch (Exception ex)
        {
            return FormatError($"Fetch failed: {ex.Message}");
        }

        if (!preview.Success)
        {
            return FormatError(preview.ErrorMessage ?? "Unknown error fetching preview");
        }

        return
            "WEB_PREVIEW:" + Environment.NewLine +
            $"url={preview.Url}" + Environment.NewLine +
            $"domain={preview.Domain}" + Environment.NewLine +
            $"title={preview.Title}" + Environment.NewLine +
            $"description={preview.Description}" + Environment.NewLine +
            $"site_name={preview.SiteName}" + Environment.NewLine +
            $"image_url={preview.ImageUrl}" + Environment.NewLine +
            $"favicon_url={preview.FaviconUrl}";
    }

    private static string GetRequired(IReadOnlyDictionary<string, string> arguments, string name)
    {
        if (!arguments.TryGetValue(name, out var value) || string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Missing required argument: {name}");
        }

        return value.Trim();
    }

    private static string FormatError(string message)
    {
        return "WEB_PREVIEW_ERROR:" + Environment.NewLine +
               $"success=false" + Environment.NewLine +
               $"message={message}";
    }

    public string GetToolExample()
    {
        return
@"Tool: read_web_preview

Arguments:
- url: string (required, absolute URL)

Notes:
- Fetches structured metadata from a webpage.
- Extracts OpenGraph, Twitter card, and HTML fallback metadata.
- Returns title, description, image, site name, domain, and favicon.
- Does NOT return full page content (use read_webpage for that).

Failure points:
- Invalid URL format
- Network timeout or connection failure
- Website blocks automated requests
- Non-HTML responses
- Missing or incomplete metadata
- Server errors (4xx/5xx)";
    }
}