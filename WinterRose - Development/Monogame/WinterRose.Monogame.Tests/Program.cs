using System;
using WinterRose;
using WinterRose.AnonymousTypes;
using WinterRose.Monogame;
using WinterRose.Plugins;
using windows = WinterRose.Windows;

Type[] assets = TypeWorker.FindTypesWithBase<Asset>();

foreach (Type t in assets)
{
    var asset = (Asset)ActivatorExtra.CreateInstance(t);
    if (asset != null)
        asset.Save();
}

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