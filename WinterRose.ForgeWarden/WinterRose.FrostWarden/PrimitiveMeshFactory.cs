using Raylib_cs;

namespace WinterRose.ForgeWarden;

public static class PrimitiveMeshFactory
{
    public static Mesh CreateCube(float size = 1f)
    {
        Mesh mesh = Raylib.GenMeshCube(size, size, size);
        return mesh;
    }
}
