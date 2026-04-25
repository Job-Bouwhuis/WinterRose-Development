namespace WinterRose.ForgeWarden.TileMaps;

public class Biome
{
    private readonly Sprite? fallbackSprite;
    private readonly List<BiomeGroundVariant> groundVariants = new();
    private readonly List<BiomeFeatureDefinition> pathFeatures = new();
    private readonly List<BiomeFeatureDefinition> detailFeatures = new();
    private readonly Dictionary<string, BiomeTileDefinition> tileDefinitionsById = new(StringComparer.OrdinalIgnoreCase);

    public Biome(SpriteSheet sprites)
    {
        fallbackSprite = sprites;
        RegisterGroundVariant(new BiomeGroundVariant(BiomeTileDefinition.FromSprite(sprites, layer: 0, id: sprites.Source), 1f));
    }

    public Biome(Sprite sprite)
    {
        fallbackSprite = sprite;
        RegisterGroundVariant(new BiomeGroundVariant(BiomeTileDefinition.FromSprite(sprite, layer: 0, id: sprite.Source), 1f));
    }

    protected Biome() { }

    public virtual void GenerateTile(int x, int y, TileCell cell, PerlinNoise noise)
    {
        if (cell == null)
            return;

        if (groundVariants.Count == 0)
        {
            if (fallbackSprite != null)
                cell.PlaceNew(fallbackSprite.Source, fallbackSprite);
            return;
        }

        float noiseValue = SampleSmoothedNoise(noise, x, y, sampleOffset: 6);
        var selected = SelectGroundVariant(noiseValue);
        if (selected == null)
            return;

        PlaceTileAtCell(cell, selected.Tile, replaceExistingOnLayer: true);
    }

    public virtual void GeneratePathTile(int x, int y, TileCell cell, PerlinNoise noise)
    {
        GeneratePathStage(x, y, cell, noise);
    }

    public virtual void GenerateDetailTile(int x, int y, TileCell cell, PerlinNoise noise)
    {
        GenerateFeatureStage(detailFeatures, x, y, cell, noise, 347, enforceDetailSpawnRules: true);
    }

    public Biome RegisterGroundVariant(BiomeGroundVariant variant)
    {
        if (variant == null)
            throw new ArgumentNullException(nameof(variant));

        groundVariants.Add(variant);
        RegisterDefinitionTree(variant.Tile);
        return this;
    }

    public Biome ClearGroundVariants()
    {
        groundVariants.Clear();
        return this;
    }

    public Biome RegisterPathFeature(BiomeFeatureDefinition feature)
    {
        if (feature == null)
            throw new ArgumentNullException(nameof(feature));

        pathFeatures.Add(feature);
        RegisterDefinitionTree(feature.RootTile);
        return this;
    }

    public Biome RegisterDetailFeature(BiomeFeatureDefinition feature)
    {
        if (feature == null)
            throw new ArgumentNullException(nameof(feature));

        detailFeatures.Add(feature);
        RegisterDefinitionTree(feature.RootTile);
        return this;
    }

    private BiomeGroundVariant? SelectGroundVariant(float noiseValue)
    {
        var candidates = new List<BiomeGroundVariant>();
        float total = 0f;

        for (int i = 0; i < groundVariants.Count; i++)
        {
            var variant = groundVariants[i];
            if (!variant.MatchesNoise(noiseValue) || variant.Weight <= 0f)
                continue;

            candidates.Add(variant);
            total += variant.Weight;
        }

        if (candidates.Count == 0)
            return null;

        float roll = Math.Clamp(noiseValue, 0f, 1f) * total;
        float running = 0f;
        for (int i = 0; i < candidates.Count; i++)
        {
            running += candidates[i].Weight;
            if (roll <= running)
                return candidates[i];
        }

        return candidates[^1];
    }

    private void GeneratePathStage(int x, int y, TileCell cell, PerlinNoise noise)
    {
        if (pathFeatures.Count == 0 || cell?.Map == null)
            return;

        float pathField = SampleSmoothedNoise(noise, x + 1024, y - 768, sampleOffset: 14);
        float warp = (SampleSmoothedNoise(noise, x - 2048, y + 512, sampleOffset: 20) - 0.5f) * 0.35f;

        for (int i = 0; i < pathFeatures.Count; i++)
        {
            var feature = pathFeatures[i];
            float localNoise = SampleSmoothedNoise(noise, x + i * 37, y - i * 29, sampleOffset: 10);
            if (!feature.MatchesNoise(localNoise))
                continue;

            float center = 0.5f + warp;
            float laneDistance = MathF.Abs(pathField - center);
            float laneWidth = 0.028f + Math.Clamp(feature.SpawnChance, 0f, 1f) * 0.09f;

            if (laneDistance <= laneWidth)
                PlaceTileOnMap(cell.Map, x, y, feature.RootTile, feature.ReplaceExistingOnLayer);
        }
    }

    private void GenerateFeatureStage(List<BiomeFeatureDefinition> features, int x, int y, TileCell cell, PerlinNoise noise, int stageSalt, bool enforceDetailSpawnRules = false)
    {
        if (features.Count == 0 || cell?.Map == null)
            return;

        var map = cell.Map;
        float noiseValue = noise?.Get(x, y) ?? 0.5f;

        for (int i = 0; i < features.Count; i++)
        {
            var feature = features[i];
            if (!feature.MatchesNoise(noiseValue))
                continue;

            int featureSalt = stageSalt + StableHash(feature.Id);
            float rootRoll = Hash01(x, y, map.Seed, featureSalt);
            if (rootRoll > Math.Clamp(feature.SpawnChance, 0f, 1f))
                continue;

            PlaceFeature(map, feature, x, y, featureSalt, enforceDetailSpawnRules);
        }
    }

    private static float SampleSmoothedNoise(PerlinNoise? noise, int x, int y, int sampleOffset)
    {
        if (noise == null)
            return 0.5f;

        float center = noise.Get(x, y);
        float left = noise.Get(x - sampleOffset, y);
        float right = noise.Get(x + sampleOffset, y);
        float up = noise.Get(x, y - sampleOffset);
        float down = noise.Get(x, y + sampleOffset);

        float value = (center * 0.5f) + ((left + right + up + down) * 0.125f);
        return Math.Clamp(value, 0f, 1f);
    }

    private void PlaceFeature(TileMap map, BiomeFeatureDefinition feature, int originX, int originY, int baseSalt, bool enforceDetailSpawnRules)
    {
        var visited = new HashSet<long>();
        var queue = new Queue<(BiomeTileDefinition tile, int x, int y, int depth)>();
        queue.Enqueue((feature.RootTile, originX, originY, 0));
        int processedNodes = 0;
        int maxNodes = Math.Max(64, feature.MaxDepth * 64);

        while (queue.Count > 0)
        {
            if (processedNodes++ >= maxNodes)
                break;

            var node = queue.Dequeue();
            long key = PackCoord(node.x, node.y);
            if (!visited.Add(key))
                continue;

            if (node.depth > feature.MaxDepth)
                continue;

            if (map.GetBiomeAt(node.x, node.y, create: false) != this)
                continue;

            if (enforceDetailSpawnRules && !CanSpawnDetailOnTarget(map, node.x, node.y, node.tile))
                continue;

            PlaceTileOnMap(map, node.x, node.y, node.tile, feature.ReplaceExistingOnLayer);

            foreach (var sideRule in node.tile.NeighborRules)
            {
                var options = sideRule.Value;
                if (options == null || options.Count == 0)
                    continue;

                float totalChance = 0f;
                for (int i = 0; i < options.Count; i++)
                    totalChance += Math.Clamp(options[i].Chance, 0f, 1f);

                if (totalChance <= 0f)
                    continue;

                var offset = GetSideOffset(sideRule.Key);
                int nx = node.x + offset.dx;
                int ny = node.y + offset.dy;

                float roll = Hash01(nx, ny, map.Seed, baseSalt + (node.depth + 1) * 37 + (int)sideRule.Key);
                if (roll > Math.Min(1f, totalChance))
                    continue;

                float pick = roll;
                float cumulative = 0f;
                BiomeTileDefinition? selected = null;
                for (int i = 0; i < options.Count; i++)
                {
                    cumulative += Math.Clamp(options[i].Chance, 0f, 1f);
                    if (pick <= cumulative)
                    {
                        selected = options[i].Tile;
                        break;
                    }
                }

                if (selected != null)
                    queue.Enqueue((selected, nx, ny, node.depth + 1));
            }
        }
    }

    private bool CanSpawnDetailOnTarget(TileMap map, int x, int y, BiomeTileDefinition detailDefinition)
    {
        if (map.IsStructureFootprint(x, y))
            return false;

        var tiles = map.GetTilesAt(x, y);
        if (tiles.Count == 0)
            return true;

        Tile? supportTile = null;
        int supportLayer = int.MinValue;
        for (int i = 0; i < tiles.Count; i++)
        {
            var candidate = tiles[i];
            if (candidate.Layer >= detailDefinition.Layer)
                continue;

            if (candidate.Layer > supportLayer)
            {
                supportLayer = candidate.Layer;
                supportTile = candidate;
            }
        }

        if (supportTile == null)
            return true;

        if (string.IsNullOrWhiteSpace(supportTile.Id))
            return true;

        if (!tileDefinitionsById.TryGetValue(supportTile.Id, out var supportDefinition))
            return true;

        var allowed = supportDefinition.AllowedDetailIds;
        if (allowed == null || allowed.Count == 0)
            return true;

        bool containsNothingDetail = false;
        bool containsRequestedDetail = false;

        foreach (var allowedId in allowed)
        {
            if (string.Equals(allowedId, BiomeTileDefinition.NothingDetailId, StringComparison.OrdinalIgnoreCase))
                containsNothingDetail = true;

            if (string.Equals(allowedId, detailDefinition.Id, StringComparison.OrdinalIgnoreCase))
                containsRequestedDetail = true;
        }

        if (allowed.Count == 1 && containsNothingDetail)
            return false;

        return containsRequestedDetail;
    }

    private void RegisterDefinitionTree(BiomeTileDefinition root)
    {
        if (root == null)
            return;

        var stack = new Stack<BiomeTileDefinition>();
        var visitedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        stack.Push(root);

        while (stack.Count > 0)
        {
            var def = stack.Pop();
            if (string.IsNullOrWhiteSpace(def.Id))
                continue;

            if (!visitedIds.Add(def.Id))
                continue;

            if (!tileDefinitionsById.ContainsKey(def.Id))
                tileDefinitionsById[def.Id] = def;

            foreach (var sideRule in def.NeighborRules)
            {
                var options = sideRule.Value;
                if (options == null)
                    continue;

                for (int i = 0; i < options.Count; i++)
                {
                    var option = options[i];
                    if (option?.Tile != null)
                        stack.Push(option.Tile);
                }
            }
        }
    }

    private static (int dx, int dy) GetSideOffset(TileNeighborSide side)
    {
        return side switch
        {
            TileNeighborSide.Left => (-1, 0),
            TileNeighborSide.Right => (1, 0),
            TileNeighborSide.Up => (0, -1),
            TileNeighborSide.Down => (0, 1),
            _ => (0, 0)
        };
    }

    private static long PackCoord(int x, int y)
    {
        return ((long)x << 32) | (uint)y;
    }

    private static float Hash01(int x, int y, int seed, int salt)
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

    private static int StableHash(string value)
    {
        unchecked
        {
            int hash = 17;
            for (int i = 0; i < value.Length; i++)
                hash = hash * 31 + value[i];
            return hash;
        }
    }

    private static void PlaceTileAtCell(TileCell cell, BiomeTileDefinition definition, bool replaceExistingOnLayer)
    {
        if (definition == null)
            return;

        if (replaceExistingOnLayer)
            cell.RemoveTile(t => t.Layer == definition.Layer);

        var tile = definition.TileFactory();
        tile.Layer = definition.Layer;
        if (string.IsNullOrWhiteSpace(tile.Id))
            tile.Id = definition.Id;

        cell.AddTile(tile);
    }

    private static void PlaceTileOnMap(TileMap map, int x, int y, BiomeTileDefinition definition, bool replaceExistingOnLayer)
    {
        if (replaceExistingOnLayer)
        {
            while (map.RemoveTileAt(x, y, t => t.Layer == definition.Layer))
            {
            }
        }
        else if (map.FindTileAt(x, y, t => t.Layer == definition.Layer) != null)
        {
            return;
        }

        var tile = definition.TileFactory();
        tile.Layer = definition.Layer;
        if (string.IsNullOrWhiteSpace(tile.Id))
            tile.Id = definition.Id;
        map.PlaceTile(x, y, tile);
    }
}
