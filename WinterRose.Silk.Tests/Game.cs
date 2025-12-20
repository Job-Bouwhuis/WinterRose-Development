using Silk.NET.OpenGL;
using System.Drawing;
using WinterRose.SilkEngine;
using WinterRose.SilkEngine.Rendering;
using WinterRose.SilkEngine.Windowing;
using ForgeShader = WinterRose.SilkEngine.Shaders.ForgeShader;

public class Game : WindowFunctionality
{
    public EngineWindow Window { get; protected set; }

    private IVertexArray vao;
    private IShader shader;

    public override void OnLoad()
    {
        shader = RenderDevice.CreateShader("Shaders/basic.vert", "Shaders/basic.frag");

        float[] vertices =
        [
            -0.5f, -0.5f, 0.0f, 1f, 0f, 0f,
             0.5f, -0.5f, 0.0f, 0f, 1f, 0f,
             0.0f,  0.5f, 0.0f, 0f, 0f, 1f,
        ];

        var vbo = RenderDevice.CreateVertexBuffer(vertices);
        vao = RenderDevice.CreateVertexArray(vbo, new VertexAttribute[]
        {
            new VertexAttribute(0, 3, 6 * sizeof(float), 0),
            new VertexAttribute(1, 3, 6 * sizeof(float), 3 * sizeof(float)),
        });
    }

    public override void OnRender(double deltaTime)
    {
        RenderDevice.Clear(0.2f, 0.3f, 0.3f, 1.0f);
        shader.Use();
        RenderDevice.DrawTriangles(vao, 3);
    }

    public override void OnClose()
    {
    }

    public override void OnUpdate(double deltaTime) { }
    public override void OnResize(int width, int height) { }
}

