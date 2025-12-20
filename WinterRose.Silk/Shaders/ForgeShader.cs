using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.FileManagement;
using WinterRose.SilkEngine.Rendering;

namespace WinterRose.SilkEngine.Shaders;
public class ForgeShader : IShader
{
    private readonly uint _program;
    private readonly uint vertexShader;
    private readonly uint fragmentShader;
    private readonly GL gl;

    public ForgeShader(GL gl, string vertexShaderFile, string fragmentShaderFile)
    {
        this.gl = gl;

        string shaderSource = FileManager.Read(vertexShaderFile);
        vertexShader = CompileShader(GLEnum.VertexShader, shaderSource);

        shaderSource = FileManager.Read(fragmentShaderFile);
        fragmentShader = CompileShader(GLEnum.FragmentShader, shaderSource);

        _program = this.gl.CreateProgram();
        this.gl.AttachShader(_program, vertexShader);
        this.gl.AttachShader(_program, fragmentShader);
        this.gl.LinkProgram(_program);

        string programLog = this.gl.GetProgramInfoLog(_program);
        if (!string.IsNullOrEmpty(programLog))
        {
            Console.WriteLine($"Program link error: {programLog}");
        }

        //this.gl.DetachShader(_program, vertexShader);
        //this.gl.DetachShader(_program, fragmentShader);
        //this.gl.DeleteShader(vertexShader);
        //this.gl.DeleteShader(fragmentShader);
    }

    // Use the shader program
    public void Use()
    {
        gl.UseProgram(_program);
    }

    // Set a 2D vector uniform in the shader
    public void SetVector2(string name, Vector2D<float> value)
    {
        int location = gl.GetUniformLocation(_program, name);
        gl.Uniform2(location, value.X, value.Y);
    }

    public void SetInt(string name, int value)
    {
        int location = gl.GetUniformLocation(_program, name);
        gl.Uniform1(location, value);
    }

    private uint CompileShader(GLEnum type, string source)
    {
        uint shader = gl.CreateShader(type);
        gl.ShaderSource(shader, source);
        gl.CompileShader(shader);

        // Check compilation status
        string infoLog = gl.GetShaderInfoLog(shader);
        if (!string.IsNullOrEmpty(infoLog))
        {
            Console.WriteLine($"Shader compile error: {infoLog}");
        }

        return shader;
    }

    // Dispose method to delete shader program
    public void Dispose()
    {
        this.gl.DetachShader(_program, vertexShader);
        this.gl.DetachShader(_program, fragmentShader);
        this.gl.DeleteShader(vertexShader);
        this.gl.DeleteShader(fragmentShader);
        gl.DeleteProgram(_program);
    }
}