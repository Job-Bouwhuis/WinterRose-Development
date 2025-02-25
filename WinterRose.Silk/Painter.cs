using Silk.NET.Maths;
using Silk.NET.OpenGL;

using Shader = WinterRose.SilkEngine.Shaders.Shader;

namespace WinterRose.SilkEngine;

public class Painter
{
    private GL gl;
    private Shader shader;

    public Painter(GL gl, Shader shader)
    {
        this.gl = gl;
        this.shader = shader;
    }

    public unsafe void DrawSprite(Sprite sprite, Vector2D<float> position)
    {
        float[] vertices = new float[]
        {
            -0.5f, -0.5f, 0.0f, 0.0f, 1.0f,
             0.5f, -0.5f, 0.0f, 1.0f, 1.0f,
             0.5f,  0.5f, 0.0f, 1.0f, 0.0f, 

             0.5f,  0.5f, 0.0f, 1.0f, 0.0f,
            -0.5f,  0.5f, 0.0f, 0.0f, 0.0f,
            -0.5f, -0.5f, 0.0f, 0.0f, 1.0f
        };

        // Step 1: Create a vertex array object (VAO)
        uint vao = 0;
        gl.GenVertexArrays(1, &vao);
        gl.BindVertexArray(vao);

        // Step 2: Create a buffer to store the vertex data (VBO)
        uint vbo = 0;
        gl.GenBuffers(1, &vbo);
        gl.BindBuffer(GLEnum.ArrayBuffer, vbo);

        fixed (float* vertexPtr = vertices)
        {
            gl.BufferData(GLEnum.ArrayBuffer, unchecked((nuint)vertices.Length * sizeof(float)), vertexPtr, GLEnum.StaticDraw);
        }

        // Step 3: Setup vertex attributes (position, texture coordinates)
        gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 5 * sizeof(float), (void*)0);
        gl.EnableVertexAttribArray(0);

        gl.VertexAttribPointer(1, 2, GLEnum.Float, false, 5 * sizeof(float), (void*)(3 * sizeof(float)));
        gl.EnableVertexAttribArray(1);

        // Step 4: Bind the texture (make sure the texture has already been loaded into OpenGL)
        gl.ActiveTexture(GLEnum.Texture0); // Activate texture unit 0
        gl.BindTexture(GLEnum.Texture2D, sprite.TextureID); // Bind the sprite texture to the current texture unit

        // Step 5: Set the shader uniform (e.g., model, projection matrix, etc.)
        shader.Use();
        shader.SetVector2("position", position); // Pass position to shader (for example, model transformation)

        // Step 6: Draw the sprite (execute the drawing commands)
        gl.DrawArrays(GLEnum.Triangles, 0, 6); // Draw the two triangles forming the sprite

        // Step 7: Clean up (unbind the buffers and VAO to avoid state pollution)
        gl.BindVertexArray(0);
        gl.BindBuffer(GLEnum.ArrayBuffer, 0);
        gl.BindTexture(GLEnum.Texture2D, 0); // Unbind texture

        // Optionally, delete the VAO and VBO if you want to clean up after drawing
        gl.DeleteVertexArrays(1, &vao);
        gl.DeleteBuffers(1, &vbo);
    }
}
