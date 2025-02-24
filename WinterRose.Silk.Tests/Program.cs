using Silk.NET.Maths;

namespace WinterRose.SilkEngine.Tests;

public class Program : Application
{
    Painter painter;
    Sprite sprite;

    Vector2D<float> position = new(50, 100);

    public static void Main()
    {
        var game = new Program();
        game.Run();
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        painter = new Painter(gl, new Shaders.Shader(gl, "Shaders/basic.vert", "Shaders/basic.frag"));
        sprite = new Sprite("CardboardBoxIcon.png", gl);
    }

    protected override void OnUpdate(double deltaTime)
    {
        position = position with { X = position.X + 5 * (float)deltaTime };
    }

    protected override void OnRender(double deltaTime)
    {
        base.OnRender(deltaTime);

        painter.DrawSprite(sprite, position);
    }
}
