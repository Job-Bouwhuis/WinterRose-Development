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
using WinterRose.ForgeWarden.Worlds;

internal class Program() : ForgeWardenEngine(false, fancyShutdown: false)
{
    private static WinterRose.Vectors.Vector2I screenSize;
    readonly Vector2 HeartCenter = new Vector2(screenSize.X / 2 - 25 / 2, 25);

    private ContentStyle style = new(new StyleBase());

    private static void Main(string[] args)
    {
        screenSize = Windows.GetScreenSize(0);

        new Program().RunAsOverlay("Gift App", 0);
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
        {
            double time;
            if (currentMessage.Length > 300)
                time = Settings.HeartDisplayTime.TotalSeconds + (currentMessage.Length - 300) / 4;
            else
                time = Settings.HeartDisplayTime.TotalSeconds;

            return time;
        }

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
            currentMessage = GenerateStyledMessage(messages.GetRandomMessage());
            currentMessage.FontSize = 24;
            time = 0;

              
            string GenerateStyledMessage(string message)
            {
                var sb = new StringBuilder();

                sb.Append("\\c[#FF9EBB]");

                // --- Typewriter roll ---
                if (Raylib.GetRandomValue(0, 100) < 20)
                {
                    sb.Append("\\tw[]");
                    sb.Append(message);
                    time += currentMessage.Length * 0.1f;
                    return sb.ToString();
                }

                // --- Guarantee at least ONE modifier ---
                var modifiers = new List<string>
                {
                    "\\bold[]",
                    "\\wave[]",
                    "\\shake[]"
                };

                int guaranteedIndex = Raylib.GetRandomValue(0, modifiers.Count - 1);
                sb.Append(modifiers[guaranteedIndex]);

                // --- Optional extra rolls (can include the guaranteed one again, so skip it) ---
                for (int i = 0; i < modifiers.Count; i++)
                {
                    if (i == guaranteedIndex)
                        continue;

                    if (Raylib.GetRandomValue(0, 100) < 30) // tuning knob
                        sb.Append(modifiers[i]);
                }

                sb.Append(message);

                return sb.ToString();
            }
        }
    }

    public override void Draw()
    {
        if (morph != null)
        {
            morph.Draw();
            if(morph.IsCompleted)
            {
                var textWidth = RichTextRenderer.MeasureRichText(currentMessage, 100).Size.X;
                textWidth = MathF.Min(textWidth, 250);
                RichTextRenderer.DrawRichText(currentMessage, HeartCenter 
                    with { 
                        X = HeartCenter.X - textWidth / 2,
                        Y = HeartCenter.Y + 25 
                    }, 100 + textWidth, style, null);
            }
        }
        else
            shape?.Draw();
    }
}