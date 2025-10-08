using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden;
public class MeshRenderer : Component, IRenderable
{
    [Hide]
    public Mesh mesh;
    [Hide]
    public Material material = Raylib.LoadMaterialDefault();
    public Color color = Color.White;

    public void Draw(Matrix4x4 viewMatrix)
    {
        if (mesh.VaoId == 0) return;
        Raylib.DrawMesh(mesh, material, transform.worldMatrix);
    }
}
