using ContentPageAndSectionTests.PageContentSystem.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace ContentPageAndSectionTests.PageContentSystem;

public sealed class ContentPageContext(NavigationManager Nav, string PageKey) : IAsyncDisposable
{
    private readonly Dictionary<string, HashSet<ContentSection>> SECTIONS = [];

    private HubConnection? HUB_CONNECTION;

    public string PageKey { get; internal set; } = PageKey;

    public void Register(ContentSection SECTION)
    {
        if (!SECTIONS.TryGetValue(SECTION.Key, out HashSet<ContentSection>? REGISTERED_SECTIONS))
        {
            REGISTERED_SECTIONS = [];
            SECTIONS.Add(SECTION.Key, REGISTERED_SECTIONS);
        }

        REGISTERED_SECTIONS.Add(SECTION);
    }

    public void Unregister(ContentSection SECTION)
    {
        if (!SECTIONS.TryGetValue(SECTION.Key, out HashSet<ContentSection>? REGISTERED_SECTIONS))
            return;

        REGISTERED_SECTIONS.Remove(SECTION);

        if (REGISTERED_SECTIONS.Count == 0)
            SECTIONS.Remove(SECTION.Key);
    }

    public async Task NotifyInitialize()
    {
        HUB_CONNECTION = new HubConnectionBuilder()
            .WithUrl(Nav.ToAbsoluteUri("/hubs/content"))
            .WithAutomaticReconnect()
            .Build();

        HUB_CONNECTION.On<ContentChangedMessage>(
            "ContentChanged",
            NotifyChangedAsync);

        await HUB_CONNECTION.StartAsync();

        ContentSection[] REGISTERED = SECTIONS
            .Values
            .SelectMany(SECTION => SECTION)
            .Distinct()
            .ToArray();

        foreach (ContentSection SECTION in REGISTERED)
            await SECTION.RefreshAsync();
    }

    private async Task NotifyChangedAsync(ContentChangedMessage MESSAGE)
    {
        foreach (string KEY in MESSAGE.Keys)
        {
            if (!SECTIONS.TryGetValue(KEY, out HashSet<ContentSection>? REGISTERED_SECTIONS))
                continue;

            ContentSection[] SECTIONS_TO_REFRESH = [.. REGISTERED_SECTIONS];

            foreach (ContentSection SECTION in SECTIONS_TO_REFRESH)
                await SECTION.RefreshAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (HUB_CONNECTION is null)
            return;

        await HUB_CONNECTION.DisposeAsync();
    }
}