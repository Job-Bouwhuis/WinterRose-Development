using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace WinterRose.ForgeWarden.TileMaps;

public class TileMap : Component, IUpdatable, IRenderable
{
    public int TileSize { get; private set; } = 64;

    // optional number of logical layers (not strictly enforced — tiles carry their own Layer)
    public int LayerCount { get; private set; } = 8;

    public int RegionSize { get; private set; } = 32; // tiles per region default

    public int Seed { get; set; } = 0;
    public bool GenerateBiomesOnRegionCreate { get; set; } = true;

    readonly Dictionary<long, TileRegion> regions = new();

    public PerlinNoise Noise { get; set; }

    public void Initialize(int tileSize = 256, int regionSize = 32, int layerCount = 8)
    {
        if (tileSize <= 0 || regionSize <= 0) throw new ArgumentException("invalid tilemap dimensions");
        TileSize = tileSize;
        RegionSize = Math.Max(1, regionSize);
        LayerCount = Math.Max(1, layerCount);

        regions.Clear();

        // seed an origin region so immediate queries / painting don't need to create external regions first
        var origin = GetOrCreateRegion(0, 0);
        origin.State = LoadedState.Loaded;
    }

    float PatternNoise(float x, float y, float seed = 0f)
    {
        float nx = x * 0.05f + seed;
        float ny = y * 0.05f + seed * 0.5f;

        float v = MathF.Sin(nx) * MathF.Cos(ny);
        v += MathF.Sin(ny * 0.5f + nx * 0.3f);
        v += MathF.Cos(nx * 0.8f - ny * 0.6f);
        v *= 0.33f;

        return (v * 0.5f) + 0.5f;
    }

    void NotifyNeighborsChange(int x, int y)
    {
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = x + dx;
                int ny = y + dy;
                var neighborCell = GetTileCell(nx, ny, create: false);
                if (neighborCell == null) continue;
                var list = neighborCell.Tiles;
                for (int i = 0; i < list.Count; i++)
                {
                    // call the virtual neighbour method on the tile
                    list[i].OnNeighborChanged(x, y);
                }
            }
        }
    }

    // --- REPLACE PlaceTile: let the region notify the tile about placement ---
    public bool PlaceTile(int x, int y, Tile tile)
    {
        var cell = GetTileCell(x, y, create: true);
        if (cell == null) return false;

        // compute region + local coords
        var (rx, ry) = RegionCoordFromTile(x, y);
        var (localX, localY) = LocalCoordInRegion(x, y);
        var region = GetOrCreateRegion(rx, ry);

        cell.AddTile(tile);
        // inform the region so it can invoke tile.OnPlaced (or custom behaviour)
        region.NotifyTilePlaced(localX, localY, tile, x, y);

        if (region.State == LoadedState.Unloaded) region.State = LoadedState.Loaded;

        NotifyNeighborsChange(x, y);
        return true;
    }

    bool TryGetRegion(int rx, int ry, out TileRegion region)
    {
        return regions.TryGetValue(PackRegionKey(rx, ry), out region);
    }

    TileCell GetTileCell(int gx, int gy, bool create = false)
    {
        var (rx, ry) = RegionCoordFromTile(gx, gy);
        var (localX, localY) = LocalCoordInRegion(gx, gy);
        if (create)
        {
            var region = GetOrCreateRegion(rx, ry);
            return region.GetCell(localX, localY);
        }
        if (TryGetRegion(rx, ry, out var r)) return r.GetCell(localX, localY);
        return null;
    }

    // --- REPLACE RemoveTileAt: let the region notify the tile about removal ---
    public bool RemoveTileAt(int x, int y, Predicate<Tile> match)
    {
        var cell = GetTileCell(x, y, create: false);
        if (cell == null) return false;

        var (rx, ry) = RegionCoordFromTile(x, y);
        var (localX, localY) = LocalCoordInRegion(x, y);
        if (!TryGetRegion(rx, ry, out var region)) region = null;

        for (int i = cell.Tiles.Count - 1; i >= 0; i--)
        {
            if (match(cell.Tiles[i]))
            {
                var removed = cell.Tiles[i];
                cell.Tiles.RemoveAt(i);
                // inform the region (if available) so it can call OnRemoved
                region?.NotifyTileRemoved(localX, localY, removed, x, y);
                NotifyNeighborsChange(x, y);
                return true;
            }
        }
        return false;
    }

    public void SetTileAt(int x, int y, Tile tile)
    {
        if (tile == null)
            throw new ArgumentNullException(nameof(tile));

        // fetch or create the appropriate region for this coordinate
        TileCell cell = GetTileCell(x, y, create: true);
        TileRegion region = GetOrCreateRegion(TileToRegionX(x), TileToRegionY(y));

        // remove any previous top tile of the same layer (optional but prevents stacking conflicts)
        Tile existing = cell.GetTopTile();
        if (existing != null && existing.Layer == tile.Layer)
            cell.RemoveTile(t => true);

        cell.AddTile(tile);

        // mark region as dirty for saving
        region.IsDirty = true;

        // notify neighbors (still handled by the tilemap itself)
        NotifyNeighborsChange(x, y);
   }

    int TileToRegionX(int x) => x / RegionSize;
    int TileToRegionY(int y) => y / RegionSize;

    public Vector2 GridToWorld(int gx, int gy, bool center = true)
    {
        float wx = gx * TileSize;
        float wy = gy * TileSize;
        if (center) { wx += TileSize * 0.5f; wy += TileSize * 0.5f; }
        return new Vector2(wx, wy);
    }

    public (int gx, int gy) WorldToGrid(Vector2 worldPos)
    {
        int gx = (int)Math.Floor(worldPos.X / TileSize);
        int gy = (int)Math.Floor(worldPos.Y / TileSize);
        return (gx, gy);
    }

    public IReadOnlyList<Tile> GetTilesAt(int x, int y)
    {
        var cell = GetTileCell(x, y, create: false);
        if (cell == null) return Array.Empty<Tile>();
        return cell.Tiles.AsReadOnly();
    }

    public Tile GetTopTileAt(int x, int y)
    {
        var cell = GetTileCell(x, y, create: false);
        return cell?.GetTopTile();
    }

    public Tile FindTileAt(int x, int y, Predicate<Tile> match)
    {
        var cell = GetTileCell(x, y, create: false);
        if (cell == null) return null;
        var list = cell.Tiles;
        for (int i = list.Count - 1; i >= 0; i--)
            if (match(list[i])) return list[i];
        return null;
    }

    // iterate cells with a callback
    public void ForEachCell(Action<int, int, TileCell> action)
    {
        // iterate only existing regions (can't iterate infinite space)
        foreach (var kv in regions)
        {
            var region = kv.Value;
            int baseTileX = region.RegionX * region.Size;
            int baseTileY = region.RegionY * region.Size;
            for (int ly = 0; ly < region.Size; ly++)
            {
                for (int lx = 0; lx < region.Size; lx++)
                {
                    int gx = baseTileX + lx;
                    int gy = baseTileY + ly;
                    action(gx, gy, region.GetCell(lx, ly));
                }
            }
        }
    }

    // ---- biome support (very small placeholder) ----
    public Biome GetBiomeAt(int x, int y, bool create = false)
    {
        var cell = GetTileCell(x, y, create);
        return cell?.Biome;
    }

    public void SetBiomeAt(int x, int y, Biome biome)
    {
        var cell = GetTileCell(x, y, create: true);
        if (cell != null) cell.Biome = biome;
    }

    public void UpdateRegionLoadingAroundCamera(int radiusInRegions = 7)
    {
        Vector2 camWorld;
        if (Camera.main is null)
            camWorld = Application.Current.Window.Size / 2;
        else
            camWorld = Camera.main.transform.position.Vec2();

        var (centerGx, centerGy) = WorldToGrid(camWorld);
        var (centerRx, centerRy) = RegionCoordFromTile(centerGx, centerGy);

        // compute new active set
        var activeNow = new HashSet<long>();
        for (int ry = centerRy - radiusInRegions; ry <= centerRy + radiusInRegions; ry++)
        {
            for (int rx = centerRx - radiusInRegions; rx <= centerRx + radiusInRegions; rx++)
            {
                var region = GetOrCreateRegion(rx, ry);
                long key = PackRegionKey(rx, ry);
                activeNow.Add(key);

                if (region.State == LoadedState.Unloaded || region.State == LoadedState.Loading)
                {
                    region.State = LoadedState.Loaded; // transition to Loaded
                }

                if (region.State != LoadedState.Persisted)
                {
                    region.State = LoadedState.Active;
                }
            }
        }

        // deactivate previously active regions that are not in activeNow and not Persisted
        var keys = new List<long>(regions.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            long key = keys[i];
            if (activeNow.Contains(key)) continue;
            var region = regions[key];
            if (region.State == LoadedState.Active)
            {
                region.State = LoadedState.Loaded;
            }
        }
    }

    static long PackRegionKey(int rx, int ry) => ((long)rx << 32) | (uint)ry;

    // floor-division helper for negative coordinates
    static int FloorDiv(int value, int divisor)
    {
        double d = (double)value / divisor;
        return (int)Math.Floor(d);
    }

    (int regionX, int regionY) RegionCoordFromTile(int gx, int gy)
    {
        int rx = FloorDiv(gx, RegionSize);
        int ry = FloorDiv(gy, RegionSize);
        return (rx, ry);
    }

    (int localX, int localY) LocalCoordInRegion(int gx, int gy)
    {
        int rx = FloorDiv(gx, RegionSize);
        int localX = gx - rx * RegionSize;
        int ry = FloorDiv(gy, RegionSize);
        int localY = gy - ry * RegionSize;
        return (localX, localY);
    }

    TileRegion GetOrCreateRegion(int rx, int ry)
    {
        long key = PackRegionKey(rx, ry);
        if (!regions.TryGetValue(key, out var region))
        {
            region = new TileRegion(rx, ry, RegionSize, this);
            regions[key] = region;

            if (GenerateBiomesOnRegionCreate)
            {
                InitializeRegionBiomes(region);
                // mark newly generated region as loaded so it's available without extra steps
                region.State = LoadedState.Loaded;
            }
        }
        return region;
    }

    void InitializeRegionBiomes(TileRegion region)
    {
        if(Noise is null)
        {
            var baseNoise = new PerlinNoise(12345, 100f, 4);
            var detailNoise = new PerlinNoise(67890, 20f, 2);
            var combined = baseNoise.Combine(detailNoise, (a, b) => a * 0.8f + b * 0.2f);
            Noise = combined;
        }

        int baseTileX = region.RegionX * region.Size;
        int baseTileY = region.RegionY * region.Size;

        for (int ly = 0; ly < region.Size; ly++)
        {
            for (int lx = 0; lx < region.Size; lx++)
            {
                int gx = baseTileX + lx;
                int gy = baseTileY + ly;

                var cell = region.GetCell(lx, ly);
                if (cell == null) continue;

                float noise = PatternNoise(gx, gy, Seed);
                var biome = BiomeRegistry.Get(noise);
                cell.Biome = biome;

                biome.GenerateTile(gx, gy, cell, Noise);
            }
        }
    }

    public void SetRegionPersisted(int regionX, int regionY, bool persist)
    {
        var region = GetOrCreateRegion(regionX, regionY);
        if (persist)
        {
            region.State = LoadedState.Persisted;
        }
        else
        {
            if (region.State == LoadedState.Persisted) region.State = LoadedState.Loaded;
        }
    }

    IEnumerable<TileRegion> ActiveRegions()
    {
        foreach (var kv in regions)
        {
            var r = kv.Value;
            if (r.State == LoadedState.Active || r.State == LoadedState.Persisted) yield return r;
        }
    }

    // ---- update / draw ----
    public void Update()
    {
        // default streaming behaviour: ensure couple regions around camera are active
        UpdateRegionLoadingAroundCamera(radiusInRegions: 1);

        // let regions handle ticking for active/persisted regions
        foreach (var region in ActiveRegions())
        {
            region.Update();
        }
    }

    // --- REPLACE Draw: delegate per-region drawing to the region itself ---
    public void Draw(Matrix4x4 viewMatrix)
    {
        // pass a small delegate to convert grid-to-world positions
        Func<int, int, Vector2> gridToWorld = (gx, gy) => GridToWorld(gx, gy, center: true);

        foreach (var region in ActiveRegions())
        {
            region.Draw(viewMatrix, TileSize, gridToWorld);
        }
    }
}
