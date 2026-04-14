using Raylib_cs;
using System.Numerics;

namespace WinterRose.ForgeWarden.TileMaps;

public class TileRegion
{
    public readonly int RegionX;
    public readonly int RegionY;
    public LoadedState State;
    public readonly int Size; // tiles per side
    [Hide]
    public readonly TileCell[] Cells;

    public bool IsDirty { get; internal set; }
    public TileMap Map { get; internal set; }

    private RenderTexture2D compiledTexture;
    private bool hasCompiledTexture;
    private int compiledTileSize = -1;
    private bool isRenderDirty = true;
    private float lastVisitedAt = 0f;

    public TileRegion(int rx, int ry, int size, TileMap map)
    {
        Map = map;
        RegionX = rx;
        RegionY = ry;
        Size = size;
        Cells = new TileCell[Size * Size];
        for (int i = 0; i < Cells.Length; i++)
            Cells[i] = CreateTileCell();
        State = LoadedState.Unloaded;
        IsDirty = true;
        isRenderDirty = true;
    }

    private TileCell CreateTileCell()
    {
        return new TileCell()
        {
            Map = Map
        };
    }

    public TileCell GetCell(int localX, int localY)
    {
        if (localX < 0 || localY < 0 || localX >= Size || localY >= Size) return null;
        return Cells[localY * Size + localX];
    }

    public virtual void Load()
    {
    }

    public virtual void Save()
    {
    }

    public virtual void Update()
    {
        bool foundDirtyTile = false;

        for (int ly = 0; ly < Size; ly++)
        {
            for (int lx = 0; lx < Size; lx++)
            {
                var cell = GetCell(lx, ly);
                if (cell == null) continue;
                var list = cell.Tiles;
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].Tick();
                    if (list[i].Dirty)
                    {
                        list[i].Dirty = false;
                        foundDirtyTile = true;
                    }
                }
            }
        }

        if (foundDirtyTile)
            MarkRenderDirty();
    }

    public virtual void Draw(Matrix4x4 viewMatrix, int tileSize, Func<int, int, Vector2> gridToWorld)
    {
        EnsureCompiledTexture(tileSize);
        if (isRenderDirty)
            RebuildCompiledTexture(viewMatrix, tileSize);

        int baseTileX = RegionX * Size;
        int baseTileY = RegionY * Size;
        float half = tileSize * 0.5f;
        Vector2 regionWorldCenter = gridToWorld(baseTileX, baseTileY);

        Raylib.DrawTexturePro(
            compiledTexture.Texture,
            new Rectangle(0, 0, compiledTexture.Texture.Width, -compiledTexture.Texture.Height),
            new Rectangle(regionWorldCenter.X - half, regionWorldCenter.Y - half, Size * tileSize, Size * tileSize),
            Vector2.Zero,
            0f,
            Color.White);
    }

    private void EnsureCompiledTexture(int tileSize)
    {
        if (hasCompiledTexture && compiledTileSize == tileSize && compiledTexture.Id != 0)
            return;

        if (hasCompiledTexture && compiledTexture.Id != 0)
        {
            Raylib.UnloadRenderTexture(compiledTexture);
            hasCompiledTexture = false;
        }

        int sizeInPixels = Size * tileSize;
        compiledTexture = Raylib.LoadRenderTexture(sizeInPixels, sizeInPixels);
        compiledTileSize = tileSize;
        hasCompiledTexture = true;
        isRenderDirty = true;
    }

    private void RebuildCompiledTexture(Matrix4x4 viewMatrix, int tileSize)
    {
        if (!hasCompiledTexture || compiledTexture.Id == 0)
            return;

        Raylib.BeginTextureMode(compiledTexture);
        Raylib.ClearBackground(new Color(0, 0, 0, 0));

        int baseTileX = RegionX * Size;
        int baseTileY = RegionY * Size;
        float half = tileSize * 0.5f;

        for (int ly = 0; ly < Size; ly++)
        {
            for (int lx = 0; lx < Size; lx++)
            {
                var cell = GetCell(lx, ly);
                if (cell == null || cell.Tiles.Count == 0) continue;

                int gx = baseTileX + lx;
                int gy = baseTileY + ly;
                Vector2 worldPos = new(lx * tileSize + half, ly * tileSize + half);

                var list = cell.Tiles;
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].Draw(viewMatrix, worldPos, new(gx, gy), tileSize);
                }
            }
        }

        Raylib.EndTextureMode();
        isRenderDirty = false;
    }

    public virtual void NotifyTilePlaced(int localX, int localY, Tile tile, int globalX, int globalY)
    {
        tile.OnPlaced(globalX, globalY);
        MarkRenderDirty();
        IsDirty = true;
    }

    public virtual void NotifyTileRemoved(int localX, int localY, Tile tile, int globalX, int globalY)
    {
        tile.OnRemoved(globalX, globalY);
        MarkRenderDirty();
        IsDirty = true;
    }

    internal void MarkRenderDirty()
    {
        isRenderDirty = true;
    }

    internal void MarkVisited(float now)
    {
        lastVisitedAt = now;
    }

    internal void ReleaseCompiledTextureIfStale(float now, float staleAfterSeconds)
    {
        if (!hasCompiledTexture || compiledTexture.Id == 0)
            return;

        if (State == LoadedState.Active || State == LoadedState.Persisted)
            return;

        float idleTime = now - lastVisitedAt;
        if (idleTime < staleAfterSeconds)
            return;

        UnloadCompiledTexture();
    }

    internal void Destroy()
    {
        Save();
        UnloadCompiledTexture();
    }

    private void UnloadCompiledTexture()
    {
        if (!hasCompiledTexture || compiledTexture.Id == 0)
            return;

        Raylib.UnloadRenderTexture(compiledTexture);
        hasCompiledTexture = false;
        compiledTileSize = -1;
        isRenderDirty = true;
    }
}
