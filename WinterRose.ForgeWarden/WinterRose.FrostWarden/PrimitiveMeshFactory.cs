using Raylib_cs;

namespace WinterRose.ForgeWarden;

public static class PrimitiveMeshFactory
{
    public static Mesh CreateCube(float size = 1f)
    {
        // Raylib has a helper for this, but we can also generate it manually later if you prefer
        Mesh mesh = Raylib.GenMeshCube(size, size, size);
        return mesh;
    }
}
