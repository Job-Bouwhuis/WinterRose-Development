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

    // called when region should load its data (can be overridden/filled later)
    public virtual void Load()
    {
        // default: no-op (user can override)
    }

    // called when region should save its data (can be overridden/filled later)
    public virtual void Save()
    {
        // default: no-op (user can override)
    }

    // called from TileMap.Update() — ticks tiles inside this region
    public virtual void Update()
    {
        // tick every tile in region (in tile-layer order as stored in TileCell)
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
                }
            }
        }
    }

    // called from TileMap.Draw() — draws tiles inside this region
    // gridToWorld: delegate provided by TileMap to convert grid coords to world pos
    public virtual void Draw(System.Numerics.Matrix4x4 viewMatrix, int tileSize, Func<int, int, System.Numerics.Vector2> gridToWorld)
    {
        int baseTileX = RegionX * Size;
        int baseTileY = RegionY * Size;

        for (int ly = 0; ly < Size; ly++)
        {
            for (int lx = 0; lx < Size; lx++)
            {
                var cell = GetCell(lx, ly);
                if (cell == null || cell.Tiles.Count == 0) continue;

                int gx = baseTileX + lx;
                int gy = baseTileY + ly;
                var worldPos = gridToWorld(gx, gy);

                var list = cell.Tiles;
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].Draw(viewMatrix, worldPos, new(gx, gy), tileSize);
                }
            }
        }
    }

    // called by TileMap when a tile was placed into this region
    public virtual void NotifyTilePlaced(int localX, int localY, Tile tile, int globalX, int globalY)
    {
        // default behaviour: call tile's placed handler
        tile.OnPlaced(globalX, globalY);
    }

    // called by TileMap when a tile was removed from this region
    public virtual void NotifyTileRemoved(int localX, int localY, Tile tile, int globalX, int globalY)
    {
        // default behaviour: call tile's removed handler
        tile.OnRemoved(globalX, globalY);
    }
}
