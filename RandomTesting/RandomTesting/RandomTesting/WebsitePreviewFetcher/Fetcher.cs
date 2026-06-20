using System.Net;
using System.Text.RegularExpressions;

namespace RandomTesting.WebsitePreviewFetcher;

public class WebsitePreview
{
    public string Url { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
    public string SiteName { get; set; }
    public string FaviconUrl { get; set; }
    public string Domain { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
}

public class WebsitePreviewFetcher
{
    private readonly HttpClient _httpClient;
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);

    public WebsitePreviewFetcher()
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 5
        };

        _httpClient = new HttpClient(handler)
        {
            Timeout = _timeout
        };

        // Set default headers to mimic a browser
        _httpClient.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        _httpClient.DefaultRequestHeaders.Add("Accept",
            "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
        _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
        _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
    }

    public async Task<WebsitePreview> FetchPreviewAsync(string url)
    {
        var preview = new WebsitePreview { Url = url };

        try
        {
            // Validate URL
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                preview.ErrorMessage = "Invalid URL format";
                preview.Success = false;
                return preview;
            }

            preview.Domain = uri.Host;

            // Download HTML content
            var htmlContent = await DownloadHtmlAsync(url);
            if (string.IsNullOrEmpty(htmlContent))
            {
                preview.ErrorMessage = "Failed to retrieve website content";
                preview.Success = false;
                return preview;
            }

            // Extract metadata
            var metadata = ExtractMetadata(htmlContent, url);

            preview.Title = metadata.Title;
            preview.Description = metadata.Description;
            preview.ImageUrl = metadata.ImageUrl;
            preview.SiteName = metadata.SiteName ?? preview.Domain;
            preview.FaviconUrl = GetFaviconUrl(uri);
            preview.Success = true;

            return preview;
        }
        catch (TaskCanceledException)
        {
            preview.ErrorMessage = "Request timed out";
            preview.Success = false;
            return preview;
        }
        catch (Exception ex)
        {
            preview.ErrorMessage = $"Error: {ex.Message}";
            preview.Success = false;
            return preview;
        }
    }

    private async Task<string> DownloadHtmlAsync(string url)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            // Get content type and validate
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "";
            if (!contentType.Contains("html", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            // Read content as string (auto-detects encoding)
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }
        catch
        {
            return null;
        }
    }

    private (string Title, string Description, string ImageUrl, string SiteName) ExtractMetadata(string html, string baseUrl)
    {
        var title = "";
        var description = "";
        var imageUrl = "";
        var siteName = "";

        // First, try Open Graph meta tags (used by Discord/Facebook/Twitter)
        title = GetMetaContent(html, "og:title");
        description = GetMetaContent(html, "og:description");
        imageUrl = GetMetaContent(html, "og:image");
        siteName = GetMetaContent(html, "og:site_name");

        // If no Open Graph title, try Twitter card
        if (string.IsNullOrEmpty(title))
            title = GetMetaContent(html, "twitter:title");

        // If still no title, try regular HTML title
        if (string.IsNullOrEmpty(title))
            title = GetHtmlTitle(html);

        // If no OG description, try Twitter description
        if (string.IsNullOrEmpty(description))
            description = GetMetaContent(html, "twitter:description");

        // If no description, try meta description
        if (string.IsNullOrEmpty(description))
            description = GetMetaContent(html, "description");

        // If no OG image, try Twitter image
        if (string.IsNullOrEmpty(imageUrl))
            imageUrl = GetMetaContent(html, "twitter:image");

        // Clean up and validate image URL
        if (!string.IsNullOrEmpty(imageUrl))
            imageUrl = MakeAbsoluteUrl(imageUrl, baseUrl);

        // Clean up title
        title = CleanText(title);
        description = CleanText(description);

        // Fallback for empty title
        if (string.IsNullOrEmpty(title))
        {
            title = "No Title";
        }

        // Limit description length (Discord shows ~200 chars)
        if (!string.IsNullOrEmpty(description))
            description = description[..500] + "...";

        return (title, description, imageUrl, siteName);
    }

    private string GetMetaContent(string html, string property)
    {
        // Look for property attribute (og:title, etc.)
        var pattern = $@"<meta\s+[^>]*property=[""']{Regex.Escape(property)}[""'][^>]*content=[""']([^""']*)[""']";
        var match = Regex.Match(html, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        // Some sites use name attribute instead
        pattern = $@"<meta\s+[^>]*name=[""']{Regex.Escape(property)}[""'][^>]*content=[""']([^""']*)[""']";
        match = Regex.Match(html, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        return null;
    }

    private string GetHtmlTitle(string html)
    {
        var pattern = @"<title>(.*?)</title>";
        var match = Regex.Match(html, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private string MakeAbsoluteUrl(string relativeUrl, string baseUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(relativeUrl))
                return null;

            if (Uri.TryCreate(relativeUrl, UriKind.Absolute, out Uri absoluteUri))
            {
                return relativeUrl;
            }

            if (Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri baseUri))
            {
                if (relativeUrl.StartsWith("//"))
                {
                    return $"{baseUri.Scheme}:{relativeUrl}";
                }

                if (relativeUrl.StartsWith("/"))
                {
                    return $"{baseUri.Scheme}://{baseUri.Host}{relativeUrl}";
                }

                var combined = new Uri(baseUri, relativeUrl);
                return combined.ToString();
            }
        }
        catch
        {
            // If we can't resolve, return null
        }

        return null;
    }

    private string GetFaviconUrl(Uri uri)
    {
        return $"{uri.Scheme}://{uri.Host}/favicon.ico";
    }

    private string CleanText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // Remove excessive whitespace
        text = Regex.Replace(text, @"\s+", " ");
        text = text.Trim();

        // Decode HTML entities
        text = WebUtility.HtmlDecode(text);

        return text;
    }

    // Method to download and save the preview image
    public async Task<byte[]> DownloadImageAsync(string imageUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl))
                return null;

            using var response = await _httpClient.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
                return null;

            var contentType = response.Content.Headers.ContentType?.MediaType ?? "";
            if (!contentType.StartsWith("image/"))
                return null;

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch
        {
            return null;
        }
    }
}
