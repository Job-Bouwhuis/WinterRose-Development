using Raylib_cs;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using WinterRose.ForgeWarden.Shaders;

namespace WinterRose.ForgeWarden;
public class MeshRenderer : Component, IRenderable
{
    [Hide]
    public Mesh mesh;
    [Hide]
    public Material material;
    public Color color = Color.White;

    // cache locations (optional but recommended)
    private int locLightDir = -1;
    private int locLightColor = -1;
    private int locAmbientColor = -1;

    protected override void Awake()
    {
        material = Raylib.LoadMaterialDefault();
        material.Shader = Raylib.LoadShader("vert.glsl", "frag.glsl");

        // cache uniform locations (safe to call once)
        if (material.Shader.Id != 0)
        {
            locLightDir = Raylib.GetShaderLocation(material.Shader, "lightDir");
            locLightColor = Raylib.GetShaderLocation(material.Shader, "lightColor");
            locAmbientColor = Raylib.GetShaderLocation(material.Shader, "ambientColor");
        }
    }

    public void Draw(Matrix4x4 viewMatrix)
    {
        if (mesh.VaoId == 0 || material.Shader.Id == 0) return;

        Raylib.BeginShaderMode(material.Shader);

        unsafe
        {
            // Provide vec3 uniforms (fragment expects vec3)
            // Example: directional light coming from above-front (adjust to your scene)
            Vector3 lightDir3 = new Vector3(-.2f, -.5f, -.8f);
            Vector3 lightColor3 = new Vector3(0.0f, 1.0f, 1.0f);     // red
            Vector3 ambient3 = new Vector3(0.2f, 0.2f, 0.2f);        // dim ambient

            if (locLightDir >= 0) Raylib.SetShaderValue(material.Shader, locLightDir, &lightDir3, ShaderUniformDataType.Vec3);
            if (locLightColor >= 0) Raylib.SetShaderValue(material.Shader, locLightColor, &lightColor3, ShaderUniformDataType.Vec3);
            if (locAmbientColor >= 0) Raylib.SetShaderValue(material.Shader, locAmbientColor, &ambient3, ShaderUniformDataType.Vec3);

            // Draw mesh with the Raylib matrix type
            Raylib.DrawMesh(mesh, material, transform.worldMatrix);
        }

        Raylib.EndShaderMode();
    }
}
