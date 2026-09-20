using Raylib_cs;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.Windowing;
using WinterRose.ForgeWarden.Worlds;

internal class Program : ForgeWardenEngine
{
    private static void Main(string[] args)
    {
        new Program().Run("something", 1920, 1080);
    }

    public override World CreateFirstWorld() => new("");

    public override void AfterWindowCreation()
    {
        UIWindow w = new UIWindow("Test Window", 400, 400, 100, 100);
        w.Show();

        w.AddContent(new UIText("Something \\wave[] else"));
        RichSpriteRegistry.RegisterSprite("test", Sprite.CreateCircle(25, Color.Magenta));

        w.AddContent(new UIText("25 \\s[test] damage"));
    }
}