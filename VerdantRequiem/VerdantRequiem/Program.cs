using System.Numerics;
using BulletSharp;
using Raylib_cs;
using VerdantRequiem.Configs.Controls;
using VerdantRequiem.Worlds;
using WinterRose.ArgumentUtility;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.Editor;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.UserInterface.Tooltipping;
using WinterRose.ForgeWarden.Worlds;
using WinterRose.ForgeWarden.UserInterface.Windowing;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.EventBusses;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;

namespace VerdantRequiem;

public class VerdantRequiem() : ForgeWardenEngine(UseBrowser: false)
{
    private Tooltip t;
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

        return DebugWorld.DebugLevel();
    }
}