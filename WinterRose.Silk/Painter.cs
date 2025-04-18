using Silk.NET.OpenGL;
using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Drawing;
using Shader = WinterRose.SilkEngine.Shaders.Shader;

namespace WinterRose.SilkEngine
{
    public class Painter
    {
        private readonly GL gl;
        private Shader shader;
        private uint vao, vbo, ebo;
        private List<SpriteDrawCall> drawCalls;
        private uint _currentTextureID;

        // For batching
        private const int MaxBatchSize = 10000;

        public Painter(GL gl)
        {
            this.gl = gl;
            drawCalls = new List<SpriteDrawCall>();
            InitializeBuffers();
            shader = new Shader(this.gl, "Shaders/basic.vert", "Shaders/basic.frag");
        }

        public void Begin()
        {
            drawCalls.Clear();
        }

        public void Draw(Texture texture, Vector2 position, Vector2 size, Color color)
        {
            if (drawCalls.Count >= MaxBatchSize)
            {
                End();
                Begin();
            }

            drawCalls.Add(new SpriteDrawCall
            {
                Texture = texture,
                Position = position,
                Size = size,
                Color = color
            });
        }

        public void End()
        {
            if (drawCalls.Count == 0)
                return;

            // Set the shader program
            shader.Use();

            // Bind the VAO (vertex array object)
            gl.BindVertexArray(vao);

            // Loop through all draw calls
            foreach (var call in drawCalls)
            {
                if (call.Texture.Handle != _currentTextureID)
                {
                    // Bind the new texture if necessary
                    gl.BindTexture(TextureTarget.Texture2D, call.Texture.Handle);
                    _currentTextureID = call.Texture.Handle;
                }

                // Update VBO with new sprite data
                UpdateVertexBuffer(call);

                // Draw the sprite
                gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, IntPtr.Zero);
            }

            // Unbind the VAO
            gl.BindVertexArray(0);

            // Clear the draw calls list
            drawCalls.Clear();
        }

        private unsafe void InitializeBuffers()
        {
            // Create VAO, VBO, and EBO
            vao = gl.GenVertexArray();
            vbo = gl.GenBuffer();
            ebo = gl.GenBuffer();

            gl.BindVertexArray(vao);

            // Vertices (positions, texture coords, and colors)
            float[] vertices = {
                // Positions        // Texture Coordinates    // Colors
                0.5f,  0.5f,        1.0f, 1.0f,              1.0f, 1.0f, 1.0f, 1.0f,
                0.5f, -0.5f,        1.0f, 0.0f,              1.0f, 1.0f, 1.0f, 1.0f,
               -0.5f, -0.5f,        0.0f, 0.0f,              1.0f, 1.0f, 1.0f, 1.0f,
               -0.5f,  0.5f,        0.0f, 1.0f,              1.0f, 1.0f, 1.0f, 1.0f
            };

            // Indices for the quad
            uint[] indices = {
                0, 1, 3,
                1, 2, 3
            };

            // Load data into VBO and EBO
            gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
            gl.BufferData(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), &vertices, BufferUsageARB.StaticDraw);

            gl.BindBuffer(GLEnum.ElementArrayBuffer, ebo);
            gl.BufferData(GLEnum.ElementArrayBuffer, (uint)(indices.Length * sizeof(uint)), &indices, BufferUsageARB.StaticDraw);

            // Set attribute pointers
            gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*)0);           // Position
            gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*)(2 * sizeof(float)));  // Texture Coordinates
            gl.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, 8 * sizeof(float), (void*)(4 * sizeof(float)));  // Colors

            gl.EnableVertexAttribArray(0);
            gl.EnableVertexAttribArray(1);
            gl.EnableVertexAttribArray(2);

            // Unbind the VAO
            gl.BindVertexArray(0);
        }

        private unsafe void UpdateVertexBuffer(SpriteDrawCall drawCall)
        {
            float[] vertexData = {
                // Positions               // TexCoords    // Colors
                drawCall.Position.X + drawCall.Size.X, drawCall.Position.Y + drawCall.Size.Y, 1.0f, 1.0f, drawCall.Color.R, drawCall.Color.G, drawCall.Color.B, drawCall.Color.A,
                drawCall.Position.X + drawCall.Size.X, drawCall.Position.Y,           1.0f, 0.0f, drawCall.Color.R, drawCall.Color.G, drawCall.Color.B, drawCall.Color.A,
                drawCall.Position.X,           drawCall.Position.Y,                   0.0f, 0.0f, drawCall.Color.R, drawCall.Color.G, drawCall.Color.B, drawCall.Color.A,
                drawCall.Position.X,           drawCall.Position.Y + drawCall.Size.Y, 0.0f, 1.0f, drawCall.Color.R, drawCall.Color.G, drawCall.Color.B, drawCall.Color.A
            };

            gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
            gl.BufferSubData(GLEnum.ArrayBuffer, IntPtr.Zero, (uint)(vertexData.Length * sizeof(float)), &vertexData);
        }
    }

    public struct SpriteDrawCall
    {
        public Texture Texture;
        public Vector2 Position;
        public Vector2 Size;
        public Color Color;
    }
}