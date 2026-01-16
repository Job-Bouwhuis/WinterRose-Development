using Raylib_cs;
using System.Numerics;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.Geometry;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.Worlds;

internal class Program : ForgeWardenEngine
{
    readonly Vector2 HeartCenter = new Vector2(25, 25);

    private static void Main(string[] args)
    {
        new Program().RunAsOverlay("Gift App", 1);
    }

    public override World CreateFirstWorld()
    {
        Raylib.SetTargetFPS(60);
        return new World("");
    }

    private void CreateFlower()
    {
        var cfg = new GeometricFlowerBuilder.FlowerConfig
        {
            Center = new Vector2(Window.Size.X / 2, Window.Size.Y),
        };

        shape = GeometricFlowerBuilder.Flower(cfg);
    }

    float time = 0;
    private ShapeCollection shape;
    private ShapeCollectionMorpher.MorphController morph;

    public override void Update()
    {
        time += Time.deltaTime;

        if(shape == null)
        {
            if(time > 5)
            {
                CreateFlower();
                time = 0;
            }
            return;
        }

        if (morph != null)
        {
            if(time > 5)
            {
                morph = null;
                shape = null;
                time = 0;
            }
            return;
        }
          
        if(time > 5)
        {
            var heart = GeometricHeartBuilder.Build(HeartCenter, 20, Color.Red, Color.Brown, 0);
            morph = ShapeCollectionMorpher.CreateMorph(shape, heart, 3);
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
                RichText t = "\\c[#FF9EBB]I love you!";
                t.FontSize = 24;
                RichTextRenderer.DrawRichText(t, HeartCenter with { Y = HeartCenter.Y + 25 }, 100, null);
            }
        }
        else
            shape?.Draw();
    }
}