using Microsoft.Graph.Models;
using WinterRose.EventBusses;
using static WinterRose.WindowHooks;

namespace WinterRoseUtilityApp.MailReader;

public class MailFetchProgress
{
    public int TotalFolders { get; set; }
    public int ProcessedFolders { get; set; }

    public int TotalMessages { get; set; }
    public int ProcessedMessages { get; set; }

    public string CurrentFolderName { get; set; }
    public string Stage { get; set; }

    public float OverallProgress
    {
        get
        {
            if (TotalMessages == 0)
                return 0f;

            float progress = (float)ProcessedMessages / TotalMessages;

            if (progress < 0f) return 0f;
            if (progress > 1f) return 1f;

            return progress;
        }
    }
}

public unsafe class FetchProgressInfo
{
    public Action<MailFetchProgress> OnProgress { get; }
    public MailFetchProgress CurrentProgress { get; private set; }
    public string CurrentFolderName { get; set; }
    public MailboxStats Stats { get; set; }
    public IntRef ProcessedFolders { get; set; }
    public IntRef ProcessedMessages { get; set; }

    public FetchProgressInfo(
        Action<MailFetchProgress> onProgress,
        MailFetchProgress currentProgress,
        MailboxStats stats,
        IntRef processedFolders,
        IntRef processedMessages)
    {
        OnProgress = onProgress;
        CurrentProgress = currentProgress;
        Stats = stats;
        ProcessedFolders = processedFolders;
        ProcessedMessages = processedMessages;
    }

    public void InvokeNow(string stage)
    {
        OnProgress.Invoke(CurrentProgress = new MailFetchProgress
        {
            Stage = stage,
            TotalFolders = Stats.FolderCount,
            ProcessedFolders = ProcessedFolders.Value,
            TotalMessages = Stats.TotalMessageCount,
            ProcessedMessages = ProcessedMessages.Value,
            CurrentFolderName = CurrentFolderName
        });
    }
}

