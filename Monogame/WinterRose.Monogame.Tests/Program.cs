using System;
using WinterRose;
using WinterRose.AnonymousTypes;
using WinterRose.Monogame;
using WinterRose.Plugins;
using windows = WinterRose.Windows;


try
{
    using var game = new TestApp();
    game.Run();
}
catch (PluginCompilationErrorException e)
{
    if (Debug.AllowThrow)
        throw;
    windows.CloseConsole();
    windows.MessageBox($"Plugin Compilation Error!\n\n{e.DiagnosticsString}", "Plugin Compilation Error!");
}
catch (Exception e)
{
    if (Debug.AllowThrow)
        throw;
    windows.CloseConsole();
    windows.MessageBox($"Catastrophic Error!\n\n{e.GetType().Name}\n{e.Message}", "Game Crashed!");
}