using BulletSharp;
using Raylib_cs;
using VerdantRequiem.Configs.Controls;
using WinterRose.ArgumentUtility;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.Editor;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.Worlds;

namespace VerdantRequiem;

// by default on DEBUG build mode graceful errors are disabled in the engine
public class VerdantRequiem() : ForgeWardenEngine(UseBrowser: false) 
{
    public static VerdantRequiem Instance { get; private set; }
    public static ProgramArguments CLIArgs { get; private set; }
    public const string Name = "Verdant Requiem";

    private static void Main(string[] args)
    {
        CLIArgs = args;
        VerdantRequiem app = new VerdantRequiem();
        Instance = app;

        app.Run(Name, 720, 640);
    }

    public override void AfterWindowCreation()
    {
        Window.OptimizeWindowSize();
        Raylib.SetTargetFPS(144);

        ControlBindings.BindInitial();
    }

    public override World CreateFirstWorld()
    {
#if DEBUG
        Universe.Hirarchy.Show();
#endif

        return Worlds.DebugLevel();
    }
}