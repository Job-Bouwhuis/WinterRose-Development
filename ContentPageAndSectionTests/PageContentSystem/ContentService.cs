using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using WinterRose.AnonymousTypes;
using WinterRose.WinterForgeSerializing;

namespace ContentPageAndSectionTests.PageContentSystem;

public sealed class ContentService : IDisposable
{
    private readonly IWebHostEnvironment HOST_ENVIRONMENT;
    private readonly ContentOptions OPTIONS;
    private readonly ILogger<ContentService> LOGGER;

    private readonly List<FileSystemWatcher> WATCHERS = [];
    private readonly Lock REFRESH_LOCK = new();

    public event Func<ContentChangedMessage, Task>? ContentChanged;

    private readonly IHubContext<ContentHub> HUB;

    public ContentService(
        IWebHostEnvironment HOST_ENVIRONMENT,
        IOptions<ContentOptions> OPTIONS,
        ILogger<ContentService> LOGGER,
        IHubContext<ContentHub> HUB)
    {
        this.HOST_ENVIRONMENT = HOST_ENVIRONMENT;
        this.OPTIONS = OPTIONS.Value;
        this.LOGGER = LOGGER;
        this.HUB = HUB;
    }

    public Task InitializeAsync()
    {
        if (!HOST_ENVIRONMENT.IsDevelopment())
            return Task.CompletedTask;

        foreach (string FOLDER in OPTIONS.Folders)
            SetupWatcher(FOLDER);

        return Task.CompletedTask;
    }

    private void SetupWatcher(string FOLDER)
    {
        string PATH = Path.IsPathRooted(FOLDER)
            ? FOLDER
            : Path.Combine(HOST_ENVIRONMENT.ContentRootPath, FOLDER);

        if (!Directory.Exists(PATH))
        {
            LOGGER.LogWarning(
                "Configured content folder does not exist: {Path}",
                PATH);

            return;
        }

        FileSystemWatcher WATCHER = new(PATH, "*.wf")
        {
            IncludeSubdirectories = true,
            NotifyFilter =
                NotifyFilters.FileName |
                NotifyFilters.LastWrite |
                NotifyFilters.CreationTime
        };

        WATCHER.Created += OnFileChanged;
        WATCHER.Changed += OnFileChanged;
        WATCHER.Renamed += OnFileRenamed;
        WATCHER.Deleted += OnFileChanged;

        WATCHER.EnableRaisingEvents = true;

        WATCHERS.Add(WATCHER);

        LOGGER.LogInformation(
            "Watching content folder: {Path}",
            PATH);
    }

    private void OnFileChanged(object SENDER, FileSystemEventArgs EVENT)
    {
        if (!EVENT.Name.EndsWith(".wf", StringComparison.OrdinalIgnoreCase))
            return;

        QueueContentRefresh(EVENT.FullPath);
    }

    private void OnFileRenamed(object SENDER, RenamedEventArgs EVENT)
    {
        if (!EVENT.Name.EndsWith(".wf", StringComparison.OrdinalIgnoreCase))
            return;

        QueueContentRefresh(EVENT.FullPath);
    }

    private void QueueContentRefresh(string PATH)
    {
        _ = Task.Run(async () =>
        {
            await Task.Delay(100);

            await RefreshAsync(PATH);
        });
    }

    private async Task RefreshAsync(string PATH)
    {
        string KEY = Path.GetFileNameWithoutExtension(PATH);

        if (string.IsNullOrWhiteSpace(KEY))
            return;

        await HUB.Clients.All.SendAsync(
            "ContentChanged",
            new ContentChangedMessage([KEY]));
    }

    public void Dispose()
    {
        foreach (FileSystemWatcher WATCHER in WATCHERS)
        {
            WATCHER.EnableRaisingEvents = false;
            WATCHER.Dispose();
        }

        WATCHERS.Clear();
    }

    private FileInfo? SeekContentFile(ContentPageContext context, string key)
    {
        foreach (string FOLDER in OPTIONS.Folders)
        {
            string PATH = Path.Combine(HOST_ENVIRONMENT.ContentRootPath, FOLDER, context.PageKey);
            string[] paths = Directory.GetFiles(PATH, "*.wf", SearchOption.AllDirectories);
            foreach (string page in paths)
            {
                string contentName = Path.GetFileNameWithoutExtension(page);
                if (contentName == key)
                    return new FileInfo(page);
            }
        }
        return null;
    }

    internal async Task<Anonymous> LoadContentAsync(ContentPageContext context, string key)
    {
        FileInfo? FILE = SeekContentFile(context, key);

        if (FILE == null)
            return new();

        using Stream fileStream = File.Open(FILE.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
        object? result = WinterForge.DeserializeFromHumanReadableStream(fileStream);
        if(result is not Anonymous a)
        {
            LOGGER.LogWarning("Content file {File} did not deserialize to an Anonymous type.", FILE.FullName);
            return new();
        }

        LOGGER.LogInformation("Loaded content from {File}.", FILE.FullName);
        return a;
    }
}
