using GiftApp;
using Raylib_cs;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using WinterRose;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.Geometry;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.ToastNotifications;
using WinterRose.ForgeWarden.UserInterface.Windowing;
using WinterRose.ForgeWarden.Worlds;
using static WinterRose.ForgeWarden.Geometry.GeometricFlowerBuilder;

internal class Program() : ForgeWardenEngine(false, fancyShutdown: false)
{
    private static WinterRose.Vectors.Vector2I screenSize;
    readonly Vector2 HeartCenter = new Vector2(screenSize.X / 2 - 25 / 2, 25);

    private ContentStyle style = new(new StyleBase());

    private static void Main(string[] args)
    {
        screenSize = Windows.GetScreenSize(1);

        new Program().RunAsOverlay("Gift App", 1);
    }

    public float time = 0;
    private ShapeCollection shape;
    private ShapeCollectionMorpher.MorphController morph;
    private RichText currentMessage;
    private MessageList messages;

    public override World CreateFirstWorld()
    {
        Raylib.SetTargetFPS(144);
        messages = new MessageList();
        Settings.Init();

        time = (float)Settings.FlowerInterval.TotalSeconds - 60;

        return new World("");
    }

    public override void Closing()
    {
        Settings.Shutdown();
    }

    public override void AfterWindowCreation()
    {
        Toasts.Success("Hello darling2!!!!! I love you so much! 💖💖💖💖", ToastRegion.Center, ToastStackSide.Top);
    }

    public enum DisplayState
    {
        Waiting,
        Flower,
        MorphingToHeart,
        Heart,
        MorphingAway
    }

    private DisplayState state = DisplayState.Waiting;

    private void CreateFlower()
    {
        var cfg = new FlowerConfig
        {
            Center = new Vector2(Window.Size.X / 2, Window.Size.Y),
        };

        var flower = Flower(cfg);
        shape = flower.Shapes;

        FlowerGalleryManager.RegisterGeneratedFlower(flower.ResolvedConfig, flower.Shapes);

        state = DisplayState.Flower;
    }

    private RichText GenerateStyledMessage(string message)
    {
        var sb = new StringBuilder();

        sb.Append("\\c[#FF9EBB]");

        if (Raylib.GetRandomValue(0, 100) < 20)
        {
            sb.Append("\\tw[]");
            sb.Append(message);

            return sb.ToString();
        }

        var modifiers = new List<string>
    {
        "\\bold[]",
        "\\wave[]",
        "\\shake[]"
    };

        int guaranteedIndex = Raylib.GetRandomValue(
            0,
            modifiers.Count - 1);

        sb.Append(modifiers[guaranteedIndex]);

        for (int i = 0; i < modifiers.Count; i++)
        {
            if (i == guaranteedIndex)
                continue;

            if (Raylib.GetRandomValue(0, 100) < 30)
                sb.Append(modifiers[i]);
        }

        sb.Append(message);

        return sb.ToString();
    }

    public double GetCurrentTimerSeconds()
    {
        return state switch
        {
            DisplayState.MorphingAway or DisplayState.Waiting => Settings.FlowerInterval.TotalSeconds,
            DisplayState.Flower => Settings.FlowerDisplayTime.TotalSeconds,
            DisplayState.MorphingToHeart => -1,
            DisplayState.Heart => Settings.HeartDisplayTime.TotalSeconds + (currentMessage.Length > 300 ? (currentMessage.Length - 300) / 4.0 : 0),
        };
    }

    public override void Update()
    {
        time += Time.deltaTime;

        switch (state)
        {
            case DisplayState.Waiting:
                {
                    if (time > Settings.FlowerInterval.TotalSeconds)
                    {
                        CreateFlower();
                        time = 0;
                    }

                    break;
                }

            case DisplayState.Flower:
                {
                    if (time > Settings.FlowerDisplayTime.TotalSeconds)
                    {
                        var heart = GeometricHeartBuilder.Build(
                            HeartCenter,
                            20,
                            Color.Red,
                            Color.Brown,
                            0);

                        morph = ShapeCollectionMorpher.CreateMorph(
                            shape,
                            heart,
                            1.5f);

                        currentMessage = GenerateStyledMessage(
                            messages.GetRandomMessage());

                        currentMessage.FontSize = 24;

                        state = DisplayState.MorphingToHeart;
                        time = 0;
                    }

                    break;
                }

            case DisplayState.MorphingToHeart:
                {
                    if (morph.IsCompleted)
                    {
                        shape = morph.Snapshot();
                        morph = null;

                        state = DisplayState.Heart;
                        time = 0;
                    }

                    break;
                }

            case DisplayState.Heart:
                {
                    if (time > GetCurrentTimerSeconds())
                    {
                        currentMessage = null;

                        morph = ShapeCollectionMorpher.CreateMorph(
                            shape,
                            new ShapeCollection(),
                            1.5f);

                        state = DisplayState.MorphingAway;
                        time = 0;
                    }

                    break;
                }

            case DisplayState.MorphingAway:
                {
                    if (morph.IsCompleted)
                    {
                        morph = null;
                        shape = null;

                        state = DisplayState.Waiting;
                        time = 0;
                    }

                    break;
                }
        }
    }

    public override void Draw()
    {
        if (morph != null)
            morph.Draw();
        else
            shape?.Draw();

        if ((state == DisplayState.Heart ||
             state == DisplayState.MorphingAway) &&
            currentMessage != null)
        {
            var textWidth =
                RichTextRenderer.MeasureRichText(
                    currentMessage,
                    100).Size.X;

            textWidth = MathF.Min(textWidth, 250);

            RichTextRenderer.DrawRichText(
                currentMessage,
                HeartCenter with
                {
                    X = HeartCenter.X - textWidth / 2,
                    Y = HeartCenter.Y + 25
                },
                100 + textWidth,
                style,
                null);
        }
    }
}