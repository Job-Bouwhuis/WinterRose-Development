using System.Diagnostics;

namespace LocalCodexAgent;

public sealed class OllamaRuntimeManager : IDisposable
{
    private readonly HttpClient httpClient;
    private readonly Uri baseUri;
    private Process? serverProcess;

    public bool StartedByThisApp { get; private set; }

    public OllamaRuntimeManager(Uri baseUri)
    {
        this.httpClient = new HttpClient(){
            Timeout = TimeSpan.FromMinutes(5)
        };
        this.baseUri = baseUri;
    }

    public async Task EnsureRunningAsync(CancellationToken cancellationToken)
    {
        if (await IsRunningAsync(cancellationToken))
        {
            return;
        }

        StartServerProcess();
        StartedByThisApp = true;

        await WaitUntilRunningAsync(cancellationToken);
    }

    public async Task<bool> IsRunningAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync(new Uri(baseUri, "api/tags"), cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task RequestShutdownAsync(CancellationToken cancellationToken)
    {
        if (!StartedByThisApp)
        {
            return;
        }

        if (serverProcess is null)
        {
            return;
        }

        try
        {
            if (!serverProcess.HasExited)
            {
                serverProcess.Kill(entireProcessTree: true);
                await serverProcess.WaitForExitAsync(cancellationToken);
            }
        }
        catch
        {
        }
    }

    private void StartServerProcess()
    {
        var processFileName = GetExecutableName();

        var startInfo = new ProcessStartInfo
        {
            FileName = processFileName,
            Arguments = "serve",
            WorkingDirectory = AppContext.BaseDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        var process = new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };

        process.OutputDataReceived += (sender, args) => { };
        process.ErrorDataReceived += (sender, args) => { };

        if (!process.Start())
        {
            throw new InvalidOperationException("Failed to start Ollama.");
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        serverProcess = process;
    }

    private async Task WaitUntilRunningAsync(CancellationToken cancellationToken)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(60);

        while (DateTimeOffset.UtcNow < deadline)
        {
            if (await IsRunningAsync(cancellationToken))
            {
                return;
            }

            await Task.Delay(500, cancellationToken);
        }

        throw new TimeoutException("Ollama did not become ready in time.");
    }

    private static string GetExecutableName()
    {
        if (OperatingSystem.IsWindows())
        {
            return "ollama.exe";
        }

        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            return "ollama";
        }

        throw new PlatformNotSupportedException("Unsupported operating system.");
    }

    public void Dispose()
    {
        serverProcess?.Dispose();
    }
}