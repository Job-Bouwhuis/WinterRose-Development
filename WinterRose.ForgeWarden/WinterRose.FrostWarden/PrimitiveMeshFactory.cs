using Raylib_cs;

namespace WinterRose.ForgeWarden;

public static class PrimitiveMeshFactory
{
    public static Mesh CreateCube(float size = 1f)
    {
        Mesh mesh = Raylib.GenMeshCube(size, size, size);
        return mesh;
    }

    public static Mesh CreateTorus(float radius = 1f, float size = 0.3f, int rings = 24, int slices = 32)
    {
        Mesh mesh = Raylib.GenMeshTorus(radius, size, rings, slices);
        return mesh;
    }

}
