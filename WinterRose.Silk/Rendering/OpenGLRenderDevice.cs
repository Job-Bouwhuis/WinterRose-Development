using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.SilkEngine.Shaders;

namespace WinterRose.SilkEngine.Rendering;

public unsafe class OpenGLRenderDevice : IRenderDevice
{
    private readonly GL gl;

    public OpenGLRenderDevice(GL gl) => this.gl = gl;

    public void Clear(float r, float g, float b, float a)
    {
        gl.ClearColor(r, g, b, a);
        gl.Clear((uint)(GLEnum.ColorBufferBit | GLEnum.DepthBufferBit));
    }

    public IBuffer CreateVertexBuffer(float[] data)
    {
        uint vbo = gl.GenBuffer();
        gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        unsafe
        {
            fixed (float* ptr = &data[0])
            {
                gl.BufferData(GLEnum.ArrayBuffer, (nuint)(data.Length * sizeof(float)), ptr, GLEnum.StaticDraw);
            }
        }
        gl.BindBuffer(GLEnum.ArrayBuffer, 0);
        return new GLBuffer(vbo, gl);
    }

    public IVertexArray CreateVertexArray(IBuffer vertexBuffer, VertexAttribute[] attributes)
    {
        uint vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);

        vertexBuffer.Bind(gl);

        foreach (var attr in attributes)
        {
            gl.EnableVertexAttribArray((uint)attr.Index);
            gl.VertexAttribPointer((uint)attr.Index, attr.Size, GLEnum.Float, false, (uint)attr.Stride, (void*)(attr.Offset));
        }

        gl.BindBuffer(GLEnum.ArrayBuffer, 0);
        gl.BindVertexArray(0);

        return new GLVertexArray(vao, gl);
    }

    public IShader CreateShader(string vertexPath, string fragmentPath) => new ForgeShader(gl, vertexPath, fragmentPath);

    public void DrawTriangles(IVertexArray vao, uint count)
    {
        vao.Bind(gl);
        gl.DrawArrays(GLEnum.Triangles, 0, count);
    }

    public void SetViewport(int x, int y, uint width, uint height) => gl.Viewport(x, y, width, height);

    public void Dispose() { /* delete buffers/shaders if needed */ }
}

// Adapter classes
public class GLBuffer : IBuffer
{
    private readonly uint handle;
    private readonly GL gl;
    public GLBuffer(uint handle, GL gl) { this.handle = handle; this.gl = gl; }
    public void Bind(GL gl) => gl.BindBuffer(GLEnum.ArrayBuffer, handle);
    public void Dispose() => gl.DeleteBuffer(handle);
}

public class GLVertexArray : IVertexArray
{
    private readonly uint handle;
    private readonly GL gl;
    public GLVertexArray(uint handle, GL gl) { this.handle = handle; this.gl = gl; }
    public void Bind(GL gl) => gl.BindVertexArray(handle);
    public void Dispose() => gl.DeleteVertexArray(handle);
}

