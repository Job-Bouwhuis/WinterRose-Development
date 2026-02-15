namespace WinterRose.ForgeWarden.TextRendering.EscapeSequences;

using System;
using System.Collections.Generic;

/// <summary>
/// Registry for managing all escape sequence handlers.
/// Provides centralized handler lookup and registration for extensibility.
/// </summary>
public static class EscapeSequenceHandlerRegistry
{
    private static readonly Dictionary<string, EscapeSequenceHandler> Handlers = new();

    static EscapeSequenceHandlerRegistry()
    {
        // Register all built-in handlers
        RegisterHandler(new ColorSequenceHandler());
        RegisterHandler(new SpriteSequenceHandler());
        RegisterHandler(new LinkSequenceHandler());
        RegisterHandler(new SpinnerSequenceHandler());
        
        // Register text style handlers
        RegisterHandler(new BoldSequenceHandler());
        RegisterHandler(new ItalicSequenceHandler());
        
        // Register animation effect handlers
        RegisterHandler(new WaveSequenceHandler());
        RegisterHandler(new ShakeSequenceHandler());
        RegisterHandler(new TypewriterSequenceHandler());
        
        // Register utility element handlers
        RegisterHandler(new ProgressBarSequenceHandler());
        RegisterHandler(new TooltipSequenceHandler());
        
        // Register interactive element handlers
        RegisterHandler(new FunctionSequenceHandler());
        RegisterHandler(new ButtonSequenceHandler());
    }

    /// <summary>
    /// Registers a new escape sequence handler.
    /// </summary>
    public static void RegisterHandler(EscapeSequenceHandler handler)
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        Handlers[handler.PrimaryKeyword] = handler;

        foreach (var altKeyword in handler.AlternativeKeywords)
        {
            Handlers[altKeyword] = handler;
        }
    }

    /// <summary>
    /// Gets the handler for a given keyword, or null if not found.
    /// </summary>
    public static EscapeSequenceHandler GetHandler(string keyword)
    {
        Handlers.TryGetValue(keyword, out var handler);
        return handler;
    }

    /// <summary>
    /// Checks if a keyword has a registered handler.
    /// </summary>
    public static bool HasHandler(string keyword)
    {
        return Handlers.ContainsKey(keyword);
    }
}
