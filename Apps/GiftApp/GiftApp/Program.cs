using GiftApp;
using Raylib_cs;
using System.Numerics;
using System.Runtime.InteropServices;
using WinterRose;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.Geometry;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.Worlds;

internal class Program() : ForgeWardenEngine(false)
{
    private static WinterRose.Vectors.Vector2I screenSize;
    readonly Vector2 HeartCenter = new Vector2(screenSize.X / 2 - 25 / 2, 25);

    private static void Main(string[] args)
    {
        screenSize = Windows.GetScreenSize(0);

        new Program().RunAsOverlay("Gift App", 0);
    }

    public float time = 0;
    private ShapeCollection shape;
    private ShapeCollectionMorpher.MorphController morph;
    private string currentMessage;
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

    private void CreateFlower()
    {
        var cfg = new GeometricFlowerBuilder.FlowerConfig
        {
            Center = new Vector2(Window.Size.X / 2, Window.Size.Y),
        };

        shape = GeometricFlowerBuilder.Flower(cfg);
    }

    public double GetCurrentTimerSeconds()
    {
        if (shape == null)
            return Settings.FlowerInterval.TotalSeconds;

        if (morph != null)
            if (currentMessage.Length > 300)
                return Settings.HeartDisplayTime.TotalSeconds + (currentMessage.Length - 300) / 4;
            else
                return Settings.HeartDisplayTime.TotalSeconds;

        return Settings.FlowerDisplayTime.TotalSeconds;

        Vector2 test = new(1, 2);
        byte[] bytes = Marshal.PackStruct(test);
    }


    public override void Update()
    {
        time += Time.deltaTime;

        if (shape == null)
        {
            if(time > Settings.FlowerInterval.TotalSeconds)
            {
                CreateFlower();
                time = 0;
            }
            return;
        }

        if (morph != null)
        {
            if(time > GetCurrentTimerSeconds())
            {
                morph = null;
                shape = null;
                time = 0;
            }
            return;
        }
          
        if(time > Settings.FlowerDisplayTime.TotalSeconds)
        {
            var heart = GeometricHeartBuilder.Build(HeartCenter, 20, Color.Red, Color.Brown, 0);
            morph = ShapeCollectionMorpher.CreateMorph(shape, heart, 1.5f);
            currentMessage = messages.GetRandomMessage();
            time = 0;
        }
    }

    public override void Draw()
    {
        if (morph != null)
        {
            morph.Draw();
            if(morph.IsCompleted)
            {
                RichText t = "\\c[#FF9EBB]" + currentMessage;
                t.FontSize = 24;
                var textWidth = RichTextRenderer.MeasureRichText(t, 100).Size.X;
                textWidth = MathF.Min(textWidth, 250);
                RichTextRenderer.DrawRichText(t, HeartCenter 
                    with { 
                        X = HeartCenter.X - textWidth / 2,
                        Y = HeartCenter.Y + 25 
                    }, 100 + textWidth, null);
            }
        }
        else
            shape?.Draw();
    }
}