using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Linq;
using WinterRose.ConsoleExtentions;
using WinterRose.Monogame.Worlds;

namespace WinterRose.Monogame.Tests;

internal class AppCloser : ObjectBehavior
{
    public AppCloser() { }
    public AppCloser(string s) 
    { 

    }

    private void Update()
    {
        var keys = Input.GetAnyKey();
        if (keys.Pressedkeys.Contains(Keys.Escape))
        {
            try
            {
                Console.WriteLine("Saving...");
                var sw = Stopwatch.StartNew();
                sw.Stop();
                Console.WriteLine($"Saved world in {sw.Elapsed.TotalMilliseconds}ms");
                WorldTemplateCreator.CreateSave("Content/Saves/WorldSave.MonoWorld", Universe.CurrentWorld, new WorldTemplateTypeSearchOverride(typeof(AppCloser), "AppCloseur", (obj, Identifyer) =>
                {
                    return $"{Identifyer}(\"patattt\")";
                }));
            }
            catch (Exception ex)
            {
                ConsoleS.WriteErrorLine("Couldnt Save: " + ex.Message);
            }
            MonoUtils.RestartApp();
        }
    }
}
