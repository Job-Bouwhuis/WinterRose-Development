using Microsoft.Xna.Framework.Input;
using System.IO;

namespace WinterRose.Monogame.Tests;

internal class SaveDeleter : ObjectBehavior
{
    AppCloser closer;

    private void Update()
    {
        Debug.Button("knoppie", () =>
        {
            MonoUtils.RestartApp();
        });
        if (!Directory.Exists("Content/Saves")) return;
        if (Input.GetKeyDown(Keys.Back))
        {
            Directory.Delete("Content/Saves", true);
            MonoUtils.RestartApp();
        }


    }
}
