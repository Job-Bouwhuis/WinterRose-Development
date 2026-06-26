using System.Net;
using System.Text;
using AngleSharp;
using AngleSharp.Dom;

namespace RandomTesting.LocalCodex.Commands;

public sealed class GoogleSearchCommand : IAgentCommand
{
    public string Name => "google_search";
    public string Description => "Performs a Google search and returns structured results.";
    public bool IsReadonly => true;

    private static readonly HttpClient CLIENT = new();
    private const int DEFAULT_RESULT_COUNT = 5;
    private const int MAX_RESULT_COUNT = 10;

    public async Task<string> ExecuteAsync(
        AgentCommandContext context,
        IReadOnlyDictionary<string, string> arguments,
        string thought,
        CancellationToken cancellationToken)
    {
        var query = GetRequired(arguments, "query");
        var resultCountRaw = GetOptional(arguments, "num");

        var resultCount = DEFAULT_RESULT_COUNT;

        if (!string.IsNullOrWhiteSpace(resultCountRaw) && int.TryParse(resultCountRaw, out var parsed))
        {
            resultCount = parsed;
        }

        if (resultCount < 1)
        {
            resultCount = 1;
        }

        if (resultCount > MAX_RESULT_COUNT)
        {
            resultCount = MAX_RESULT_COUNT;
        }

        var requestUrl =
            "https://www.google.com/search" +
            $"?q={Uri.EscapeDataString(query)}" +
            $"&num={resultCount}" +
            "&hl=en" +
            "&ie=UTF-8" +
            "&oe=UTF-8" +
            "&gbv=1" +
            "&pws=0" +
            "&safe=off";

        var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

        request.Headers.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36"
        );

        request.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        request.Headers.AcceptLanguage.ParseAdd("en-US,en;q=0.9");

        HttpResponseMessage response;

        try
        {
            response = await CLIENT.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            return FormatError($"Search request failed: {ex.Message}");
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return FormatError($"Google search failed: {(int)response.StatusCode} {response.ReasonPhrase}. {body}");
        }

        var html = await response.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(html))
        {
            return FormatError("Google returned an empty response");
        }

        var results = await ExtractSearchResultsAsync(html, resultCount);

        var output = new StringBuilder();
        output.AppendLine("GOOGLE_SEARCH:");
        output.AppendLine($"query={query}");
        output.AppendLine($"result_count={results.Count}");

        if (results.Count == 0)
        {
            output.AppendLine("message=No usable result links found");
            return output.ToString().TrimEnd();
        }

        for (var i = 0; i < results.Count; i++)
        {
            var index = i + 1;
            var result = results[i];

            output.AppendLine($"result_{index}_title={result.Title}");
            output.AppendLine($"result_{index}_url={result.Url}");
            output.AppendLine($"result_{index}_display_url={GetDisplayUrl(result.Url)}");
        }

        return output.ToString().TrimEnd();
    }

    public string GetToolExample()
    {
        return
    @"Tool: google_search

Arguments:
- query: string (required, search query to send to Google)
- num: int (optional, default = 5; number of results to return, clamped to 1-10)

Notes:
- Performs a standard Google web search request and parses the returned HTML using a DOM parser (AngleSharp).
- Extracts search result links and titles from the rendered document structure.
- Filters out internal Google navigation and non-result links.
- No API key is required.
- This method depends on Google’s HTML structure and may break if the layout or anti-bot protections change.

Failure points:
- This tool is currently non functional due to Google’s anti-bot measures and may return no results or an error.
- Missing query argument
- Network timeout or connectivity issues
- Google returns a consent / anti-bot page instead of results
- Google HTML structure changes
- No valid search result links found
- HTML parsing failure

Output format:
- Returns GOOGLE_SEARCH block containing:
  - query
  - result_count
  - result_N_title
  - result_N_url
  - result_N_display_url";
    }

    private static async Task<List<SearchResult>> ExtractSearchResultsAsync(string html, int maxResults)
    {
        var results = new List<SearchResult>();
        var seenUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var config = Configuration.Default;
        var context = BrowsingContext.New(config);

        var document = await context.OpenAsync(req => req.Content(html));

        var links = document.QuerySelectorAll("a");

        foreach (var link in links)
        {
            if (results.Count >= maxResults)
            {
                break;
            }

            var href = link.GetAttribute("href");

            if (!TryExtractResultUrl(href, out var url))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            if (seenUrls.Contains(url))
            {
                continue;
            }

            if (IsGoogleOwnedUrl(url))
            {
                continue;
            }

            var titleElement = link.QuerySelector("h3");
            var title = CleanValue(titleElement?.TextContent ?? link.TextContent);

            if (string.IsNullOrWhiteSpace(title))
            {
                continue;
            }

            seenUrls.Add(url);
            results.Add(new SearchResult(title, url));
        }

        return results;
    }

    private static bool TryExtractResultUrl(string rawHref, out string url)
    {
        url = string.Empty;

        if (string.IsNullOrWhiteSpace(rawHref))
        {
            return false;
        }

        rawHref = WebUtility.HtmlDecode(rawHref);

        if (rawHref.StartsWith("/url?q=", StringComparison.OrdinalIgnoreCase))
        {
            var query = rawHref[(rawHref.IndexOf('?') + 1)..];
            var values = ParseQueryString(query);

            if (values.TryGetValue("q", out var extracted) &&
                Uri.TryCreate(extracted, UriKind.Absolute, out var parsed))
            {
                url = parsed.ToString();
                return true;
            }

            return false;
        }

        if (Uri.TryCreate(rawHref, UriKind.Absolute, out var direct))
        {
            url = direct.ToString();
            return true;
        }

        return false;
    }

    private static bool IsGoogleOwnedUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var parsed))
        {
            return true;
        }

        var host = parsed.Host;

        return host.EndsWith("google.com", StringComparison.OrdinalIgnoreCase) ||
               host.EndsWith("googleusercontent.com", StringComparison.OrdinalIgnoreCase) ||
               host.EndsWith("gstatic.com", StringComparison.OrdinalIgnoreCase) ||
               host.EndsWith("youtube.com", StringComparison.OrdinalIgnoreCase) ||
               host.EndsWith("youtu.be", StringComparison.OrdinalIgnoreCase);
    }

    private static Dictionary<string, string> ParseQueryString(string query)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(query))
        {
            return values;
        }

        if (query.StartsWith("?"))
        {
            query = query[1..];
        }

        var parts = query.Split('&', StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            var index = part.IndexOf('=');

            if (index < 0)
            {
                continue;
            }

            var key = WebUtility.UrlDecode(part[..index]) ?? string.Empty;
            var value = WebUtility.UrlDecode(part[(index + 1)..]) ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(key) && !values.ContainsKey(key))
            {
                values[key] = value;
            }
        }

        return values;
    }

    private static string GetDisplayUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var parsed))
        {
            return url;
        }

        return parsed.Host + parsed.AbsolutePath;
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
        if (!arguments.TryGetValue(name, out var value) || string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Trim();
    }

    private static string CleanValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Trim();
    }

    private static string FormatError(string message)
    {
        return
            "GOOGLE_SEARCH_ERROR:" + Environment.NewLine +
            "success=false" + Environment.NewLine +
            $"message={message}";
    }

    private readonly record struct SearchResult(string Title, string Url);
}