using BulletSharp;
using Microsoft.DiaSymReader;
using Raylib_cs;
using System.Globalization;
using System.Runtime.InteropServices;
using WinterRose.ForgeSignal;
using WinterRose.ForgeWarden.AssetPipeline;
using WinterRose.ForgeWarden.Components;
using WinterRose.ForgeWarden.DamageSystem;
using WinterRose.ForgeWarden.DamageSystem.WeaponSystem;
using WinterRose.ForgeWarden.Editor;
using WinterRose.ForgeWarden.Entities;
using WinterRose.ForgeWarden.HealthSystem;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.Physics;
using WinterRose.ForgeWarden.Shaders;
using WinterRose.ForgeWarden.StatusSystem;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.TileMaps;
using WinterRose.ForgeWarden.Tweens;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.Content;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes;
using WinterRose.ForgeWarden.UserInterface.DragDrop;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.ForgeWarden.UserInterface.Windowing;
using WinterRose.ForgeWarden.Worlds;
using WinterRose.FrostWarden.Tests;
using WinterRose.Recordium;
using WinterRose.StateKeeper;
using WinterRose.WinterForgeSerializing;

namespace WinterRose.ForgeWarden.Tests;

internal class Program : ForgeWardenEngine
{
    // fullscreen PC
    //const int SCREEN_WIDTH = 2560;
    //const int SCREEN_HEIGHT = 1440;

    // for on PC
    const int SCREEN_WIDTH = 1920;
    const int SCREEN_HEIGHT = 1080;

    // for on laptop
    //const int SCREEN_WIDTH = 1280;
    //const int SCREEN_HEIGHT = 720;

    //static Windows.SystemTrayIcon icon;

    // for on steam deck
    //const int SCREEN_WIDTH = 960;
    //const int SCREEN_HEIGHT = 540;

    [STAThread]
    static void Main(string[] args)
    {
        NamedControl fire = new NamedControl("fire");
        fire.AddBinding(MouseButton.Left);
        fire.AddBinding(KeyboardKey.Space);
        fire.Register();

        NamedControl reload = new("reload");
        reload.AddBinding(KeyboardKey.R);
        reload.Register();

        new Program().Run("ForgeWarden Tests", SCREEN_WIDTH, SCREEN_HEIGHT);
        //new Program().RunAsOverlay();
    }

    public override void Draw()
    {

    }

    public override World CreateFirstWorld()
    {
        Universe.Hirarchy.Show();
        ray.SetTargetFPS(144);
        World world = new World("testworld");

        // Generate region and fill with tiles using noise
        Entity tilemap = world.CreateEntity("TileMap");
        TileMap map = tilemap.AddComponent<TileMap>();


        SpriteSheet sheet = SpriteSheet.Load("Assets/spritesheet.png", 256, 256);
        Biome plains = new(sheet);
        BiomeRegistry.AddBiome(plains, 0.2f);

        Biome idk = new(Sprite.CreateRectangle(256, 256, Color.Beige));
        BiomeRegistry.AddBiome(idk, 0.8f);

        ray.SetTargetFPS(144);
        //ClearColor = Color.Beige;
        RichSpriteRegistry.RegisterSprite("star", new Sprite("bigimg"));
        RichSpriteRegistry.RegisterSprite("wf", new Sprite("tex"));

        var cam = world.CreateEntity<Camera>("cam");
        cam.transform.position = cam.transform.position with
        {
            Z = 0.6f
        };
        var entity = world.CreateEntity("entity");
        entity.transform.scale = new(0.6f);
        entity.AddComponent<ImportantComponent>();
        entity.AddComponent<SpriteRenderer>(Assets.Load<Sprite>("Rositta"));
        entity.AddComponent<CharacterController>();
        var vitals = entity.AddComponent<Vitality>();
        vitals.Health.MaxHealth = 100;
        var statusEffector = entity.AddComponent<StatusEffector>();
        
        cam.AddComponent<SmoothCamera2DMode>().Target = entity.transform;

        Universe.Hirarchy.Show();

        UIWindow dd = new UIWindow("a", 600, 500);
        dd.AddContent(new UIDateTimePicker());
        dd.Show();

        #region weapon stuff

        var projectile = world.CreateEntity<Projectile>("demoProjectile");
        var projStats = new Projectile.ProjectileStats
        {
            DamageType = new PhysicalDamage(),
            Damage = 15,
            CritChance = 50,
            CritMultiplier = 2.0f,
            StatusChance = 1.0f
        };

        // Assign stats to projectile
        projectile.Stats = projStats;


        // 5. Create trigger
        var trigger = new StandardTrigger()
        {
            FireRate = 10f
        };

        // 6. Create weapon entity
        var gun = world.CreateEntity("demoGun");

        var magazine = gun.AddComponent<Magazine>(projectile, new PerShellReloadBehavior()
        {
            ReloadTime = 5
        });
        magazine.MaxAmmo = 25;
        magazine.CurrentLoadedAmmo = 25;
        magazine.AmmoReserves = 100;

        var weapon = gun.AddComponent<Weapon>();

        weapon.Trigger = trigger;
        weapon.AvailableTriggers.Add(trigger);
        #endregion

        void ShowToast(ToastRegion r, ToastStackSide s)
        {
            var d = new Dialog("Horizontal Big",
                                "refer to \\L[https://github.com/Job-Bouwhuis/WinterRose.WinterForge|WinterForge github page] for info",
                                DialogPlacement.RightBig, priority: DialogPriority.High)
                            .AddContent(new UIButton("OK", (c, b) =>
                            {
                                ShowToast(r, s);
                                c.Close();
                            }))
                            .AddProgressBar(-1)
                            .AddSprite(Assets.Load<Sprite>("bigimg"));

            var w = new UIWindow("Graph 1.1-2-3", 400, 500, 100, 100);

            //UIGraph gall = LoadSimpleGraphFromCsv(
            //    "SorterOnePointOne.csv",
            //    "SorterOnePointTwo.csv",
            //    "SorterOnePointThree.csv"
            //    );
            //gall.XTickInterval = 25000;
            //gall.XAxisLabel = "Size";
            //gall.YAxisLabel = "Time (ms)";
            //w.AddContent(gall);

            // Graph g2 = LoadSimpleGraphFromCsv("SorterOnePointOne.csv");
            // g2.XAxisLabel = "Size";
            // g2.YAxisLabel = "Time (ms)";
            // //w.AddContent(g2);

            // Graph g3 = LoadSimpleGraphFromCsv("SorterOnePointTwo.csv");
            // g3.XAxisLabel = "Size";
            // g3.YAxisLabel = "Time (ms)";
            // //w.AddContent(g3);

            // Graph g4 = LoadSimpleGraphFromCsv("SorterOnePointThree.csv");
            // g4.XAxisLabel = "Size";
            // g4.YAxisLabel = "Time (ms)";
            // //w.AddContent(g4);

            //UIGraph g = LoadGraphFromCsv("csv.txt"); // 1.4
            //g.XTickInterval = 100;
            //g.XAxisLabel = "Threshold";
            //g.YAxisLabel = "Time (ms)";
            //w.AddContent(g);

            //UIGraph g5 = LoadGraphFromCsv("SorterOnePointFive.csv"); // 1.5
            //g5.XTickInterval = 100;
            //g5.XAxisLabel = "Threshold";
            //g5.YAxisLabel = "Time (ms)";
            //w.AddContent(g5);

            //UIGraph g7 = LoadGraphFromCsv("SorterOnePointSix.csv");
            //g7.XAxisLabel = "Size";
            //g7.YAxisLabel = "Time (ms)";
            //g7.XTickInterval = 100;
            //w.AddContent(g7);


            Toast t = new Toast(ToastType.Info, r, s)
                    //.AddText("Right?\n\n\nYes")
                    //.AddButton("btn", (t, b) => ((Toast)t).OpenAsDialog(d))
                    //.AddButton("btn2", (t, b) => Toasts.Success("Worked!", ToastRegion.Right, ToastStackSide.Bottom))
                    .AddButton("show window normal", Invocation.Create<UIContainer, UIButton>((c, b) => w.Show()))
                    .AddButton("show window collapsed", Invocation.Create<UIContainer, UIButton>((c, b) => w.ShowCollapsed()))
                    .AddButton("show window maximized", Invocation.Create<UIContainer, UIButton>((c, b) => w.ShowMaximized()))
                    .AddButton("close window", Invocation.Create<UIContainer, UIButton>((c, b) => w.Close()))
                    .AddButton("close toast", Invocation.Create<UIContainer, UIButton>((c, b) => c.Close()))
                    //.AddProgressBar(-1, infiniteSpinText: "Waiting for browser download...")
                    //.AddSprite(Assets.Load<Sprite>("bigimg"))
                    //.AddContent(new HeavyFileDropContent())
                    ;


            t.Style.TimeUntilAutoDismiss = 0;
            Toasts.ShowToast(t);
        }


        //Dialogs.Show(new Dialog("Vertical Big", "this is a cool dialog box\n\n\\s[star]\\!", DialogPlacement.HorizontalBig, priority: DialogPriority.AlwaysFirst).AddButton("Ok"));

        //Dialogs.Show(new Dialog("Dialog top left", "yes", DialogPlacement.TopLeft).AddButton("Ok"));
        //Dialogs.Show(new Dialog("Dialog top right", "yes", DialogPlacement.TopRight).AddButton("Ok"));
        //Dialogs.Show(new Dialog("Dialog bottom left", "yes", DialogPlacement.BottomLeft).AddButton("Ok"));
        //Dialogs.Show(new Dialog("Dialog bottom right", "yes", DialogPlacement.BottomRight).AddButton("Ok"));
        //Dialogs.Show(new Dialog("Dialog Center", "yes", DialogPlacement.CenterSmall).AddButton("Ok"));
        //Dialogs.Show(new Dialog("Dialog top small", "yes", DialogPlacement.TopSmall).AddButton("Ok"));
        //Dialogs.Show(new Dialog("Dialog left small", "yes", DialogPlacement.LeftSmall).AddButton("Ok"));
        //Dialogs.Show(new Dialog("Dialog right small", "yes", DialogPlacement.RightSmall).AddButton("Ok"));

        //Dialogs.Show(new Dialog("Dialog bottom small", "yes \\c[red] rode text \\c[white]  \\s[star]\\!", DialogPlacement.BottomSmall).AddButton("Ok"));

        //Dialogs.Show(new Dialog("Dialog right big", "yes", DialogPlacement.RightBig).AddButton("Ok"));

        return world;
    }

    public static UIGraph LoadSimpleGraphFromCsv(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        var graph = new UIGraph();
        var series = graph.GetSeries("Average Times");

        // skip header
        for (int i = 1; i < lines.Length; i++)
        {
            var parts = lines[i].Split(',');
            if (parts.Length != 2) continue;

            int size = int.Parse(parts[0]);
            float avg = float.Parse(parts[1], CultureInfo.InvariantCulture);

            // each line is just one point (size, avg)
            series.AddPoint(size, avg);
        }

        // give it a nice color so it doesn’t default to black or something dull
        series.Color = FromHsl(210f, 0.7f, 0.5f); // bluish tone

        return graph;
    }

    public static UIGraph LoadSimpleGraphFromCsv(params string[] filePaths)
    {
        var graph = new UIGraph();

        int fileCount = filePaths.Length;
        for (int index = 0; index < filePaths.Length; index++)
        {
            var filePath = filePaths[index];
            var lines = File.ReadAllLines(filePath);

            // series name from filename without extension
            var seriesName = Path.GetFileNameWithoutExtension(filePath)[9..];
            var series = graph.GetSeries(seriesName);

            for (int i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                if (parts.Length != 2) continue;

                int size = int.Parse(parts[0]);
                float avg = float.Parse(parts[1], CultureInfo.InvariantCulture);

                series.AddPoint(size, avg);
            }

            // assign unique color per series based on index
            float hue = (index / (float)fileCount) * 360f;
            series.Color = FromHsl(hue, 0.7f, 0.5f);
        }

        return graph;
    }


    public static UIGraph LoadGraphFromCsv(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        var dataBySize = new Dictionary<int, List<(int threshold, float avg)>>();

        // skip header
        for (int i = 1; i < lines.Length; i++)
        {
            var parts = lines[i].Split(',');
            if (parts.Length != 3) continue;

            int threshold = int.Parse(parts[0]);
            int size = int.Parse(parts[1]);
            float avg = float.Parse(parts[2], CultureInfo.InvariantCulture);

            if (!dataBySize.TryGetValue(size, out var list))
            {
                list = new List<(int, float)>();
                dataBySize[size] = list;
            }

            list.Add((threshold, avg));
        }

        // Now create the graph
        var graph = new UIGraph();

        Dictionary<string, Color> colorMap = new Dictionary<string, Color>();

        int seriesCount = dataBySize.Count;
        int index = 0;

        foreach (var kvp in dataBySize)
        {
            int size = kvp.Key;
            var series = kvp.Value
                 .GroupBy(x => x.threshold)
                 .Select(g => (threshold: g.Key, average: (float)g.Average(a => a.avg)))
                 .ToList();

            var seriesName = $"Size {size}";
            var graphSeries = graph.GetSeries(seriesName);

            foreach (var point in series)
                graphSeries.AddPoint(point.threshold, point.average);

            float hue = (index / (float)seriesCount) * 360f;
            float saturation = 0.7f;
            float lightness = 0.5f;

            Color color = FromHsl(hue, saturation, lightness);
            colorMap[seriesName] = color;
            graphSeries.Color = color;

            index++;
        }

        return graph;
    }

    static Color FromHsl(float h, float s, float l)
    {
        h /= 360f;

        float r = l, g = l, b = l;
        if (s != 0)
        {
            float temp2 = l < 0.5f ? l * (1 + s) : (l + s) - (l * s);
            float temp1 = 2 * l - temp2;

            r = HueToRgb(temp1, temp2, h + 1f / 3f);
            g = HueToRgb(temp1, temp2, h);
            b = HueToRgb(temp1, temp2, h - 1f / 3f);
        }

        return new Color((int)(r * 255), (int)(g * 255), (int)(b * 255), 255);
    }

    static float HueToRgb(float t1, float t2, float thue)
    {
        if (thue < 0) thue += 1;
        if (thue > 1) thue -= 1;
        if (6 * thue < 1) return t1 + (t2 - t1) * 6 * thue;
        if (2 * thue < 1) return t2;
        if (3 * thue < 2) return t1 + (t2 - t1) * ((2f / 3f) - thue) * 6;
        return t1;
    }
}
