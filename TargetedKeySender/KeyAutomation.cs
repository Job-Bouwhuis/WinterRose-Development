namespace TargetedKeySender;


public class KeyAutomation
{
    private bool running;

    public bool IsRunning => running;

    private TargetWindow currentWindow;
    private ushort currentKey;
    private int currentInterval;

    public string CurrentTitle => currentWindow?.Title;
    public ushort CurrentKey => currentKey;
    public int CurrentInterval => currentInterval;

    public async Task Start(TargetWindow window, ushort key, int intervalMs)
    {
        if (intervalMs < 2000)
            intervalMs = 2000;

        currentWindow = window;
        currentKey = key;
        currentInterval = intervalMs;

        running = true;

        while (running)
        {
            IntPtr previous = NativeMethods.GetForegroundWindow();

            window.Activate();
            KeySender.SendKey(key);

            // restore previous window ASAP
            if (previous != IntPtr.Zero)
                NativeMethods.SetForegroundWindow(previous);

            await Task.Delay(intervalMs);
        }
    }

    public void Stop()
    {
        running = false;
    }
}
