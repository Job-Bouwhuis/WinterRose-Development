namespace RandomTesting.LocalCodex;

public static class ConsoleCountdownPrompt
{
    public static async Task<bool?> AskYesNoAsync(
        string message,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var deadline = DateTimeOffset.UtcNow.Add(timeout);

        while (true)
        {
            var remaining = deadline - DateTimeOffset.UtcNow;

            if (remaining <= TimeSpan.Zero)
            {
                Console.WriteLine();
                return null;
            }

            var secondsLeft = (int)Math.Ceiling(remaining.TotalSeconds);
            Console.Write($"\r{message} [Y/N] auto-closing in {secondsLeft}s   ");

            bool keyAvailable;
            try
            {
                keyAvailable = Console.KeyAvailable;
            }
            catch
            {
                keyAvailable = false;
            }

            if (keyAvailable)
            {
                var key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Y)
                {
                    Console.WriteLine("Y");
                    return true;
                }

                if (key.Key == ConsoleKey.N)
                {
                    Console.WriteLine("N");
                    return false;
                }
            }

            await Task.Delay(1000, cancellationToken);
        }
    }
}