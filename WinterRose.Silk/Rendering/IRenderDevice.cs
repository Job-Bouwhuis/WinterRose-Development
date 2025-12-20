using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;

namespace WinterRose.SilkEngine.Rendering;

public interface IRenderDevice : IDisposable
{
    void Clear(float r, float g, float b, float a);
    IBuffer CreateVertexBuffer(float[] data);
    IVertexArray CreateVertexArray(IBuffer vertexBuffer, VertexAttribute[] attributes);
    IShader CreateShader(string vertexPath, string fragmentPath);
    void DrawTriangles(IVertexArray vao, uint count);
    void SetViewport(int x, int y, uint width, uint height);
}

public struct VertexAttribute
{
    public int Index;
    public int Size;       // number of components
    public int Stride;     // bytes between vertices
    public int Offset;     // bytes offset in vertex struct

    public VertexAttribute(int index, int size, int stride, int offset)
    {
        Index = index;
        Size = size;
        Stride = stride;
        Offset = offset;
    }
}

public interface IBuffer : IDisposable
{
    void Bind(GL gl);
}
public interface IVertexArray : IDisposable
{
    void Bind(GL gl);
}
public interface IShader : IDisposable
{
    void Use();
    void SetVector2(string name, Vector2D<float> value);
    void SetInt(string name, int value);
}
