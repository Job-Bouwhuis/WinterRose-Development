using System;

namespace WinterRose.Monogame;

/// <summary>
/// Provides static methods to close the game the propper way from anywhere in your code
/// </summary>
public static class ExitHelper
{
    private static Action CloseMethod = delegate { };
    private static bool _isClosing = false;
    /// <summary>
    /// Gets invoked right before the <see cref="CloseMethod"/> is invoked
    /// </summary>
    public static event Action GameClosing = delegate { };

    /// <summary>
    /// Sets the method that is invoked when the game is closed
    /// </summary>
    /// <param name="closeMethod"></param>
    public static void SetCloseMethod(Action closeMethod)
    {
        CloseMethod = closeMethod;
    }

    /// <summary>
    /// Closes the game after invoking the <see cref="GameClosing"/> event
    /// </summary>
    public static void ExitGame()
    {
        if (_isClosing) return;
        _isClosing = true;

        Console.WriteLine("Closing Game...");
        GameClosing();
        CloseMethod();
        Console.WriteLine("Bye Bye!");
    }

    internal static void InvokeGameClosingEvent()
    {
        if (_isClosing) return;
        _isClosing = true;

        GameClosing();
    }

    /// <summary>
    /// Force closes the game with code -3. This does not invoke the <see cref="GameClosing"/> event
    /// </summary>
    public static void ForceCloseGame()
    {
        Environment.Exit(-3);
    }
}
