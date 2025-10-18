using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.Shaders;

namespace WinterRose.ForgeWarden;
public class MeshRenderer : Component, IRenderable
{
    [Hide]
    public Mesh mesh;
    [Hide]
    public Material material = Raylib.LoadMaterialDefault();
    public Color color = Color.White;

    protected override void Awake()
    {
        material.Shader = ray.LoadShader("vert.glsl", "frag.glsl");
    }

    public void Draw(Matrix4x4 viewMatrix)
    {
        if (mesh.VaoId == 0) return;

        ray.BeginShaderMode(material.Shader);

        unsafe
        {
            Vector4 c = Raylib.ColorNormalize(new Color(255, 0, 0));
            Vector4 c1 = Raylib.ColorNormalize(new Color(50, 50, 50));
            
            ray.SetShaderValue(material.Shader, ray.GetShaderLocation(material.Shader, "lightColor"), &c, ShaderUniformDataType.Vec4);
            
            ray.SetShaderValue(material.Shader, ray.GetShaderLocation(material.Shader, "ambientColor"), &c1, ShaderUniformDataType.Vec4);
            Raylib.DrawMesh(mesh, material, transform.worldMatrix);
            ray.EndShaderMode();
        }
    }
}
