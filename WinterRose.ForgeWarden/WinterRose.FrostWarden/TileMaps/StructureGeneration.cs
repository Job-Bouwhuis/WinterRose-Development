namespace WinterRose.ForgeWarden.TileMaps;

public enum StructureOrientation
{
    North = 0,
    East = 1,
    South = 2,
    West = 3
}

public readonly record struct StructureBounds(int MinX, int MinY, int MaxX, int MaxY)
{
    public static readonly StructureBounds Empty = new(0, 0, 0, 0);
}

public readonly record struct StructureEntitySpawnPoint(string EntityId, int OffsetX, int OffsetY);

public readonly record struct ResolvedStructureEntitySpawn(string StructureId, string EntityId, int X, int Y);

public readonly record struct ResolvedStructureTile(
    string StructureId,
    int X,
    int Y,
    BiomeTileDefinition Definition,
    bool ReplaceExistingOnLayer,
    int Priority,
    int RuleOrder);

public sealed class StructureTileStamp
{
    private readonly BiomeTileDefinition?[] orientationTiles = new BiomeTileDefinition?[4];

    public int OffsetX { get; }
    public int OffsetY { get; }
    public bool ReplaceExistingOnLayer { get; set; }

    public StructureTileStamp(
        int offsetX,
        int offsetY,
        BiomeTileDefinition north,
        BiomeTileDefinition? east = null,
        BiomeTileDefinition? south = null,
        BiomeTileDefinition? west = null)
    {
        if (north == null)
            throw new ArgumentNullException(nameof(north));

        OffsetX = offsetX;
        OffsetY = offsetY;

        orientationTiles[(int)StructureOrientation.North] = north;
        orientationTiles[(int)StructureOrientation.East] = east ?? north;
        orientationTiles[(int)StructureOrientation.South] = south ?? north;
        orientationTiles[(int)StructureOrientation.West] = west ?? north;
    }

    public BiomeTileDefinition ResolveTile(StructureOrientation orientation)
    {
        return orientationTiles[(int)orientation]
            ?? orientationTiles[(int)StructureOrientation.North]
            ?? throw new InvalidOperationException("Structure tile stamp is missing a tile definition.");
    }
}

public interface IStructureLayout
{
    string Id { get; }

    IReadOnlyList<StructureTileStamp> TileStamps { get; }

    IReadOnlyList<StructureEntitySpawnPoint> EntitySpawnPoints { get; }

    StructureBounds GetBounds(StructureOrientation orientation);

    StructureBounds GetAnyOrientationBounds();

    IEnumerable<ResolvedStructureTile> EnumerateTiles(int originX, int originY, StructureOrientation orientation, int priority, int ruleOrder);

    IEnumerable<ResolvedStructureEntitySpawn> EnumerateEntitySpawns(int originX, int originY, StructureOrientation orientation);
}

public sealed class StructureDefinition : IStructureLayout
{
    private readonly List<StructureTileStamp> tileStamps = new();
    private readonly List<StructureEntitySpawnPoint> entitySpawnPoints = new();

    public string Id { get; }
    public IReadOnlyList<StructureTileStamp> TileStamps => tileStamps;
    public IReadOnlyList<StructureEntitySpawnPoint> EntitySpawnPoints => entitySpawnPoints;

    public StructureDefinition(string id)
    {
        Id = string.IsNullOrWhiteSpace(id) ? "structure" : id;
    }

    public StructureDefinition AddTile(
        int offsetX,
        int offsetY,
        BiomeTileDefinition north,
        BiomeTileDefinition? east = null,
        BiomeTileDefinition? south = null,
        BiomeTileDefinition? west = null,
        bool replaceExistingOnLayer = false)
    {
        var stamp = new StructureTileStamp(offsetX, offsetY, north, east, south, west)
        {
            ReplaceExistingOnLayer = replaceExistingOnLayer
        };
        tileStamps.Add(stamp);
        return this;
    }

    public StructureDefinition AddEntitySpawn(string entityId, int offsetX, int offsetY)
    {
        if (string.IsNullOrWhiteSpace(entityId))
            throw new ArgumentException("Entity id cannot be empty.", nameof(entityId));

        entitySpawnPoints.Add(new StructureEntitySpawnPoint(entityId, offsetX, offsetY));
        return this;
    }

    public StructureBounds GetBounds(StructureOrientation orientation)
    {
        if (tileStamps.Count == 0)
            return StructureBounds.Empty;

        int minX = int.MaxValue;
        int minY = int.MaxValue;
        int maxX = int.MinValue;
        int maxY = int.MinValue;

        for (int i = 0; i < tileStamps.Count; i++)
        {
            var local = StructureMath.RotateOffset(tileStamps[i].OffsetX, tileStamps[i].OffsetY, orientation);
            if (local.x < minX) minX = local.x;
            if (local.y < minY) minY = local.y;
            if (local.x > maxX) maxX = local.x;
            if (local.y > maxY) maxY = local.y;
        }

        return new StructureBounds(minX, minY, maxX, maxY);
    }

    public StructureBounds GetAnyOrientationBounds()
    {
        var north = GetBounds(StructureOrientation.North);
        var east = GetBounds(StructureOrientation.East);
        var south = GetBounds(StructureOrientation.South);
        var west = GetBounds(StructureOrientation.West);

        int minX = Math.Min(Math.Min(north.MinX, east.MinX), Math.Min(south.MinX, west.MinX));
        int minY = Math.Min(Math.Min(north.MinY, east.MinY), Math.Min(south.MinY, west.MinY));
        int maxX = Math.Max(Math.Max(north.MaxX, east.MaxX), Math.Max(south.MaxX, west.MaxX));
        int maxY = Math.Max(Math.Max(north.MaxY, east.MaxY), Math.Max(south.MaxY, west.MaxY));

        return new StructureBounds(minX, minY, maxX, maxY);
    }

    public IEnumerable<ResolvedStructureTile> EnumerateTiles(int originX, int originY, StructureOrientation orientation, int priority, int ruleOrder)
    {
        for (int i = 0; i < tileStamps.Count; i++)
        {
            var stamp = tileStamps[i];
            var offset = StructureMath.RotateOffset(stamp.OffsetX, stamp.OffsetY, orientation);
            yield return new ResolvedStructureTile(
                Id,
                originX + offset.x,
                originY + offset.y,
                stamp.ResolveTile(orientation),
                stamp.ReplaceExistingOnLayer,
                priority,
                ruleOrder);
        }
    }

    public IEnumerable<ResolvedStructureEntitySpawn> EnumerateEntitySpawns(int originX, int originY, StructureOrientation orientation)
    {
        for (int i = 0; i < entitySpawnPoints.Count; i++)
        {
            var spawn = entitySpawnPoints[i];
            var offset = StructureMath.RotateOffset(spawn.OffsetX, spawn.OffsetY, orientation);
            yield return new ResolvedStructureEntitySpawn(Id, spawn.EntityId, originX + offset.x, originY + offset.y);
        }
    }
}

public sealed class CompositePoiPart
{
    public IStructureLayout Layout { get; }
    public int OffsetX { get; }
    public int OffsetY { get; }
    public StructureOrientation OrientationOffset { get; }

    public CompositePoiPart(IStructureLayout layout, int offsetX, int offsetY, StructureOrientation orientationOffset)
    {
        Layout = layout ?? throw new ArgumentNullException(nameof(layout));
        OffsetX = offsetX;
        OffsetY = offsetY;
        OrientationOffset = orientationOffset;
    }
}

public sealed class CompositePoiDefinition : IStructureLayout
{
    private readonly List<CompositePoiPart> parts = new();
    private readonly List<StructureEntitySpawnPoint> entitySpawnPoints = new();

    public string Id { get; }
    public IReadOnlyList<StructureTileStamp> TileStamps => Array.Empty<StructureTileStamp>();
    public IReadOnlyList<StructureEntitySpawnPoint> EntitySpawnPoints => entitySpawnPoints;

    public CompositePoiDefinition(string id)
    {
        Id = string.IsNullOrWhiteSpace(id) ? "poi" : id;
    }

    public CompositePoiDefinition AddPart(IStructureLayout layout, int offsetX, int offsetY, StructureOrientation orientationOffset = StructureOrientation.North)
    {
        parts.Add(new CompositePoiPart(layout, offsetX, offsetY, orientationOffset));
        return this;
    }

    public CompositePoiDefinition AddEntitySpawn(string entityId, int offsetX, int offsetY)
    {
        if (string.IsNullOrWhiteSpace(entityId))
            throw new ArgumentException("Entity id cannot be empty.", nameof(entityId));

        entitySpawnPoints.Add(new StructureEntitySpawnPoint(entityId, offsetX, offsetY));
        return this;
    }

    public StructureBounds GetBounds(StructureOrientation orientation)
    {
        if (parts.Count == 0)
            return StructureBounds.Empty;

        int minX = int.MaxValue;
        int minY = int.MaxValue;
        int maxX = int.MinValue;
        int maxY = int.MinValue;

        for (int i = 0; i < parts.Count; i++)
        {
            var part = parts[i];
            var partOffset = StructureMath.RotateOffset(part.OffsetX, part.OffsetY, orientation);
            var partOrientation = StructureMath.CombineOrientation(orientation, part.OrientationOffset);
            var childBounds = part.Layout.GetBounds(partOrientation);

            minX = Math.Min(minX, partOffset.x + childBounds.MinX);
            minY = Math.Min(minY, partOffset.y + childBounds.MinY);
            maxX = Math.Max(maxX, partOffset.x + childBounds.MaxX);
            maxY = Math.Max(maxY, partOffset.y + childBounds.MaxY);
        }

        return new StructureBounds(minX, minY, maxX, maxY);
    }

    public StructureBounds GetAnyOrientationBounds()
    {
        var north = GetBounds(StructureOrientation.North);
        var east = GetBounds(StructureOrientation.East);
        var south = GetBounds(StructureOrientation.South);
        var west = GetBounds(StructureOrientation.West);

        int minX = Math.Min(Math.Min(north.MinX, east.MinX), Math.Min(south.MinX, west.MinX));
        int minY = Math.Min(Math.Min(north.MinY, east.MinY), Math.Min(south.MinY, west.MinY));
        int maxX = Math.Max(Math.Max(north.MaxX, east.MaxX), Math.Max(south.MaxX, west.MaxX));
        int maxY = Math.Max(Math.Max(north.MaxY, east.MaxY), Math.Max(south.MaxY, west.MaxY));

        return new StructureBounds(minX, minY, maxX, maxY);
    }

    public IEnumerable<ResolvedStructureTile> EnumerateTiles(int originX, int originY, StructureOrientation orientation, int priority, int ruleOrder)
    {
        for (int i = 0; i < parts.Count; i++)
        {
            var part = parts[i];
            var partOffset = StructureMath.RotateOffset(part.OffsetX, part.OffsetY, orientation);
            var partOriginX = originX + partOffset.x;
            var partOriginY = originY + partOffset.y;
            var partOrientation = StructureMath.CombineOrientation(orientation, part.OrientationOffset);

            foreach (var tile in part.Layout.EnumerateTiles(partOriginX, partOriginY, partOrientation, priority, ruleOrder))
                yield return tile;
        }
    }

    public IEnumerable<ResolvedStructureEntitySpawn> EnumerateEntitySpawns(int originX, int originY, StructureOrientation orientation)
    {
        for (int i = 0; i < parts.Count; i++)
        {
            var part = parts[i];
            var partOffset = StructureMath.RotateOffset(part.OffsetX, part.OffsetY, orientation);
            var partOriginX = originX + partOffset.x;
            var partOriginY = originY + partOffset.y;
            var partOrientation = StructureMath.CombineOrientation(orientation, part.OrientationOffset);

            foreach (var spawn in part.Layout.EnumerateEntitySpawns(partOriginX, partOriginY, partOrientation))
                yield return spawn;
        }

        for (int i = 0; i < entitySpawnPoints.Count; i++)
        {
            var spawn = entitySpawnPoints[i];
            var offset = StructureMath.RotateOffset(spawn.OffsetX, spawn.OffsetY, orientation);
            yield return new ResolvedStructureEntitySpawn(Id, spawn.EntityId, originX + offset.x, originY + offset.y);
        }
    }
}

public sealed class StructureSpawnRule
{
    public string Id { get; }
    public IStructureLayout Layout { get; }

    public float SpawnChance { get; set; } = 0.01f;
    public int GridSize { get; set; } = 12;
    public int Salt { get; set; }
    public int Priority { get; set; }
    public bool Enabled { get; set; } = true;
    public bool AllowRotation { get; set; } = true;
    public Func<TileMap, int, int, bool>? CanSpawnAt { get; set; }

    public StructureSpawnRule(string id, IStructureLayout layout)
    {
        Id = string.IsNullOrWhiteSpace(id) ? "structure_spawn" : id;
        Layout = layout ?? throw new ArgumentNullException(nameof(layout));
    }

    internal bool IsAnchorValid(int anchorX, int anchorY)
    {
        int grid = Math.Max(1, GridSize);
        return Mod(anchorX, grid) == 0 && Mod(anchorY, grid) == 0;
    }

    internal bool ShouldSpawn(TileMap map, int seed, int anchorX, int anchorY)
    {
        if (!Enabled || !IsAnchorValid(anchorX, anchorY))
            return false;

        float chance = Math.Clamp(SpawnChance, 0f, 1f);
        if (chance <= 0f)
            return false;

        if (CanSpawnAt != null && !CanSpawnAt(map, anchorX, anchorY))
            return false;

        return StructureMath.Hash01(anchorX, anchorY, seed, Salt) <= chance;
    }

    internal StructureOrientation ResolveOrientation(int seed, int anchorX, int anchorY)
    {
        if (!AllowRotation)
            return StructureOrientation.North;

        float roll = StructureMath.Hash01(anchorX, anchorY, seed, Salt + 9817);
        int index = (int)(roll * 4f);
        if (index < 0) index = 0;
        if (index > 3) index = 3;
        return (StructureOrientation)index;
    }

    internal int NextAlignedValue(int value)
    {
        int grid = Math.Max(1, GridSize);
        int mod = Mod(value, grid);
        return mod == 0 ? value : value + (grid - mod);
    }

    private static int Mod(int value, int modulo)
    {
        int m = value % modulo;
        return m < 0 ? m + modulo : m;
    }
}

public readonly record struct StructureRegionPlan(
    IReadOnlyList<ResolvedStructureTile> Tiles,
    IReadOnlyList<ResolvedStructureEntitySpawn> EntitySpawns,
    IReadOnlyCollection<long> FootprintTileKeys);

public sealed class StructureGenerator
{
    private readonly List<StructureSpawnRule> rules = new();

    public IReadOnlyList<StructureSpawnRule> Rules => rules;
    public bool HasRules => rules.Count > 0;

    public StructureGenerator AddRule(StructureSpawnRule rule)
    {
        if (rule == null)
            throw new ArgumentNullException(nameof(rule));

        rules.Add(rule);
        return this;
    }

    public StructureGenerator ClearRules()
    {
        rules.Clear();
        return this;
    }

    internal StructureRegionPlan PlanRegion(TileMap map, int regionX, int regionY, int seed)
    {
        if (rules.Count == 0)
            return new StructureRegionPlan(Array.Empty<ResolvedStructureTile>(), Array.Empty<ResolvedStructureEntitySpawn>(), Array.Empty<long>());

        int regionSize = map.RegionSize;
        int minX = regionX * regionSize;
        int minY = regionY * regionSize;
        int maxX = minX + regionSize - 1;
        int maxY = minY + regionSize - 1;

        var placedTiles = new Dictionary<long, ResolvedStructureTile>();
        var placedFootprint = new HashSet<long>();
        var entitySpawns = new Dictionary<long, ResolvedStructureEntitySpawn>();

        for (int ruleIndex = 0; ruleIndex < rules.Count; ruleIndex++)
        {
            var rule = rules[ruleIndex];
            if (!rule.Enabled)
                continue;

            var bounds = rule.Layout.GetAnyOrientationBounds();
            int minAnchorX = minX - bounds.MaxX;
            int maxAnchorX = maxX - bounds.MinX;
            int minAnchorY = minY - bounds.MaxY;
            int maxAnchorY = maxY - bounds.MinY;

            int startAnchorX = rule.NextAlignedValue(minAnchorX);
            int startAnchorY = rule.NextAlignedValue(minAnchorY);
            int grid = Math.Max(1, rule.GridSize);

            for (int anchorY = startAnchorY; anchorY <= maxAnchorY; anchorY += grid)
            {
                for (int anchorX = startAnchorX; anchorX <= maxAnchorX; anchorX += grid)
                {
                    if (!rule.ShouldSpawn(map, seed, anchorX, anchorY))
                        continue;

                    var orientation = rule.ResolveOrientation(seed, anchorX, anchorY);

                    foreach (var tile in rule.Layout.EnumerateTiles(anchorX, anchorY, orientation, rule.Priority, ruleIndex))
                    {
                        if (tile.X < minX || tile.X > maxX || tile.Y < minY || tile.Y > maxY)
                            continue;

                        long footprintKey = PackCoord(tile.X, tile.Y);
                        placedFootprint.Add(footprintKey);

                        long layerKey = PackCoordWithLayer(tile.X, tile.Y, tile.Definition.Layer);
                        if (placedTiles.TryGetValue(layerKey, out var existing))
                        {
                            if (tile.Priority < existing.Priority)
                                continue;

                            if (tile.Priority == existing.Priority && tile.RuleOrder > existing.RuleOrder)
                                continue;
                        }

                        placedTiles[layerKey] = tile;
                    }

                    foreach (var spawn in rule.Layout.EnumerateEntitySpawns(anchorX, anchorY, orientation))
                    {
                        if (spawn.X < minX || spawn.X > maxX || spawn.Y < minY || spawn.Y > maxY)
                            continue;

                        long spawnKey = PackEntitySpawn(spawn.EntityId, spawn.X, spawn.Y);
                        entitySpawns[spawnKey] = spawn;
                    }
                }
            }
        }

        return new StructureRegionPlan(
            placedTiles.Values.ToList(),
            entitySpawns.Values.ToList(),
            placedFootprint);
    }

    private static long PackCoord(int x, int y)
    {
        return ((long)x << 32) | (uint)y;
    }

    private static long PackCoordWithLayer(int x, int y, int layer)
    {
        unchecked
        {
            int packedLayer = layer & 0xFF;
            int shiftedY = y << 8;
            int yWithLayer = shiftedY | packedLayer;
            return ((long)x << 32) | (uint)yWithLayer;
        }
    }

    private static long PackEntitySpawn(string entityId, int x, int y)
    {
        unchecked
        {
            int idHash = StructureMath.StableHash(entityId);
            uint left = (uint)idHash;
            uint right = (uint)(((x * 397) ^ y) + 31);
            return ((long)left << 32) | right;
        }
    }
}

public static class StructurePresets
{
    public static StructureDefinition CreateWell2X2(
        string id,
        BiomeTileDefinition topLeft,
        BiomeTileDefinition topRight,
        BiomeTileDefinition bottomLeft,
        BiomeTileDefinition bottomRight,
        bool replaceExistingOnLayer = false)
    {
        if (topLeft == null) throw new ArgumentNullException(nameof(topLeft));
        if (topRight == null) throw new ArgumentNullException(nameof(topRight));
        if (bottomLeft == null) throw new ArgumentNullException(nameof(bottomLeft));
        if (bottomRight == null) throw new ArgumentNullException(nameof(bottomRight));

        // Anchor is the top-left tile of the well in North orientation.
        return new StructureDefinition(id)
            .AddTile(0, 0, topLeft, topRight, bottomRight, bottomLeft, replaceExistingOnLayer)
            .AddTile(1, 0, topRight, bottomRight, bottomLeft, topLeft, replaceExistingOnLayer)
            .AddTile(0, 1, bottomLeft, topLeft, topRight, bottomRight, replaceExistingOnLayer)
            .AddTile(1, 1, bottomRight, bottomLeft, topLeft, topRight, replaceExistingOnLayer);
    }
}

public static class StructureMath
{
    public static (int x, int y) RotateOffset(int x, int y, StructureOrientation orientation)
    {
        return orientation switch
        {
            StructureOrientation.North => (x, y),
            StructureOrientation.East => (y, -x),
            StructureOrientation.South => (-x, -y),
            StructureOrientation.West => (-y, x),
            _ => (x, y)
        };
    }

    public static StructureOrientation CombineOrientation(StructureOrientation root, StructureOrientation local)
    {
        int combined = ((int)root + (int)local) % 4;
        if (combined < 0) combined += 4;
        return (StructureOrientation)combined;
    }

    public static float Hash01(int x, int y, int seed, int salt)
    {
        unchecked
        {
            uint h = 2166136261;
            h = (h ^ (uint)x) * 16777619;
            h = (h ^ (uint)y) * 16777619;
            h = (h ^ (uint)seed) * 16777619;
            h = (h ^ (uint)salt) * 16777619;
            return (h & 0x00FFFFFF) / (float)0x01000000;
        }
    }

    public static int StableHash(string value)
    {
        unchecked
        {
            int hash = 17;
            for (int i = 0; i < value.Length; i++)
                hash = hash * 31 + value[i];
            return hash;
        }
    }
}

