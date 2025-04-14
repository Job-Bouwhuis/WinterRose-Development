using Silk.NET.OpenGL;
using System.Drawing;
using WinterRose.SilkEngine;
using Shader = WinterRose.SilkEngine.Shaders.Shader;

public class Game : Application
{
    private uint vao;
    private uint vbo;
    private Shader shader;

    protected unsafe override void Initialize()
    {
        Console.WriteLine("Initializing...");

        // Load the shader
        shader = new Shader(gl, "Shaders/basic.vert", "Shaders/basic.frag");

        float[] vertices =
        [
            // Position         // Color
            -0.5f, -0.5f, 0.0f,  1f, 0f, 0f,
             0.5f, -0.5f, 0.0f,  0f, 1f, 0f,
             0.0f,  0.5f, 0.0f,  0f, 0f, 1f,
        ];

        vao = gl.GenVertexArray();
        vbo = gl.GenBuffer();

        gl.BindVertexArray(vao);
        gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        unsafe
        {
            fixed (float* v = &vertices[0])
            {
                gl.BufferData(GLEnum.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), v, GLEnum.StaticDraw);
            }
        }

        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 6 * sizeof(float), (void*)0);

        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 3, GLEnum.Float, false, 6 * sizeof(float), (void*)(3 * sizeof(float)));

        gl.BindBuffer(GLEnum.ArrayBuffer, 0);
        gl.BindVertexArray(0);
    }

    protected override void Render(Painter painter)
    {
        gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        gl.Clear((uint)(GLEnum.ColorBufferBit | GLEnum.DepthBufferBit));

        shader.Use();
        gl.BindVertexArray(vao);
        gl.DrawArrays(GLEnum.Triangles, 0, 3);
    }

    float time = 0;
    float fps = 0;
    protected override void Update()
    {
        time += Time.deltaTime;
        fps++;
        if(time >= 1)
        {
            Console.WriteLine(fps);
            time = 0;
            fps = 0;
        }
    }
}
