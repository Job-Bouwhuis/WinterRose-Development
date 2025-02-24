using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.FileManagement;

namespace WinterRose.SilkEngine.Shaders;
public class Shader
{
    private readonly uint _program;
    private readonly GL _gl;

    public Shader(GL gl, string vertexShaderFile, string fragmentShaderFile)
    {
        _gl = gl;

        string shaderSource = FileManager.Read(vertexShaderFile);
        uint vertexShader = CompileShader(GLEnum.VertexShader, shaderSource);

        shaderSource = FileManager.Read(fragmentShaderFile);
        uint fragmentShader = CompileShader(GLEnum.FragmentShader, shaderSource);

        _program = _gl.CreateProgram();
        _gl.AttachShader(_program, vertexShader);
        _gl.AttachShader(_program, fragmentShader);
        _gl.LinkProgram(_program);
        _gl.DetachShader(_program, vertexShader);
        _gl.DetachShader(_program, fragmentShader);
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);
    }

    // Use the shader program
    public void Use()
    {
        _gl.UseProgram(_program);
    }

    // Set a 2D vector uniform in the shader
    public void SetVector2(string name, Vector2D<float> value)
    {
        int location = _gl.GetUniformLocation(_program, name);
        _gl.Uniform2(location, value.X, value.Y);
    }

    public void SetInt(string name, int value)
    {
        int location = _gl.GetUniformLocation(_program, name);
        _gl.Uniform1(location, value);
    }

    private uint CompileShader(GLEnum type, string source)
    {
        uint shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, source);
        _gl.CompileShader(shader);

        // Check compilation status
        string infoLog = _gl.GetShaderInfoLog(shader);
        if (!string.IsNullOrEmpty(infoLog))
        {
            Console.WriteLine($"Shader compile error: {infoLog}");
        }

        return shader;
    }

    // Dispose method to delete shader program
    public void Dispose()
    {
        _gl.DeleteProgram(_program);
    }
}