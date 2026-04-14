namespace WinterRose.ForgeWarden.TileMaps;

public enum TileNeighborSide
{
    Left,
    Right,
    Up,
    Down
}

public sealed class BiomeNeighborOption
{
    public BiomeTileDefinition Tile { get; }
    public float Chance { get; }

    public BiomeNeighborOption(BiomeTileDefinition tile, float chance)
    {
        Tile = tile ?? throw new ArgumentNullException(nameof(tile));
        Chance = Math.Clamp(chance, 0f, 1f);
    }
}

public sealed class BiomeTileDefinition
{
    private readonly Dictionary<TileNeighborSide, List<BiomeNeighborOption>> neighborRules = new();
    private readonly HashSet<string> allowedDetailIds = new(StringComparer.OrdinalIgnoreCase);
    private bool hasExplicitAllowedDetails;

    public const string NothingDetailId = "nothingdetail";

    public string Id { get; }
    public int Layer { get; set; }
    public Func<Tile> TileFactory { get; }
    public IReadOnlyDictionary<TileNeighborSide, List<BiomeNeighborOption>> NeighborRules => neighborRules;
    public IReadOnlyCollection<string>? AllowedDetailIds => hasExplicitAllowedDetails ? allowedDetailIds : null;

    public BiomeTileDefinition(string id, Func<Tile> tileFactory, int layer = 0)
    {
        Id = string.IsNullOrWhiteSpace(id) ? "tile" : id;
        TileFactory = tileFactory ?? throw new ArgumentNullException(nameof(tileFactory));
        Layer = layer;
    }

    public static BiomeTileDefinition FromSprite(Sprite sprite, int layer = 0, string? id = null)
    {
        if (sprite == null)
            throw new ArgumentNullException(nameof(sprite));

        string resolvedId = id ?? sprite.Source ?? "tile";
        return new BiomeTileDefinition(
            resolvedId,
            () => sprite is SpriteSheet sheet
                ? new SheetTile(sheet, layer)
                : new Tile(resolvedId, sprite, layer),
            layer);
    }

    public BiomeTileDefinition AddNeighbor(TileNeighborSide side, BiomeTileDefinition neighborTile, float chance)
    {
        if (!neighborRules.TryGetValue(side, out var list))
        {
            list = new List<BiomeNeighborOption>();
            neighborRules[side] = list;
        }

        list.Add(new BiomeNeighborOption(neighborTile, chance));
        return this;
    }

    public BiomeTileDefinition AllowDetails(params BiomeTileDefinition[] details)
    {
        allowedDetailIds.Clear();
        hasExplicitAllowedDetails = true;

        if (details == null)
            return this;

        for (int i = 0; i < details.Length; i++)
        {
            var detail = details[i];
            if (detail == null || string.IsNullOrWhiteSpace(detail.Id))
                continue;
            allowedDetailIds.Add(detail.Id);
        }

        return this;
    }

    public BiomeTileDefinition AllowDetailIds(params string[] detailIds)
    {
        allowedDetailIds.Clear();
        hasExplicitAllowedDetails = true;

        if (detailIds == null)
            return this;

        for (int i = 0; i < detailIds.Length; i++)
        {
            string id = detailIds[i];
            if (string.IsNullOrWhiteSpace(id))
                continue;
            allowedDetailIds.Add(id);
        }

        return this;
    }

    public BiomeTileDefinition AllowNoDetails()
    {
        allowedDetailIds.Clear();
        allowedDetailIds.Add(NothingDetailId);
        hasExplicitAllowedDetails = true;
        return this;
    }

    public BiomeTileDefinition AllowAnyDetails()
    {
        allowedDetailIds.Clear();
        hasExplicitAllowedDetails = false;
        return this;
    }
}

public sealed class BiomeGroundVariant
{
    public BiomeTileDefinition Tile { get; }
    public float Weight { get; }
    public float MinNoise { get; }
    public float MaxNoise { get; }

    public BiomeGroundVariant(BiomeTileDefinition tile, float weight = 1f, float minNoise = 0f, float maxNoise = 1f)
    {
        Tile = tile ?? throw new ArgumentNullException(nameof(tile));
        Weight = Math.Max(weight, 0f);
        MinNoise = Math.Clamp(minNoise, 0f, 1f);
        MaxNoise = Math.Clamp(maxNoise, 0f, 1f);
    }

    public bool MatchesNoise(float noiseValue)
    {
        return noiseValue >= MinNoise && noiseValue <= MaxNoise;
    }
}

public sealed class BiomeFeatureDefinition
{
    public string Id { get; }
    public BiomeTileDefinition RootTile { get; }
    public float SpawnChance { get; set; } = 0.1f;
    public float MinNoise { get; set; } = 0f;
    public float MaxNoise { get; set; } = 1f;
    public int MaxDepth { get; set; } = 8;
    public bool ReplaceExistingOnLayer { get; set; }

    public BiomeFeatureDefinition(string id, BiomeTileDefinition rootTile)
    {
        Id = string.IsNullOrWhiteSpace(id) ? "feature" : id;
        RootTile = rootTile ?? throw new ArgumentNullException(nameof(rootTile));
    }

    public bool MatchesNoise(float noiseValue)
    {
        float min = Math.Clamp(MinNoise, 0f, 1f);
        float max = Math.Clamp(MaxNoise, 0f, 1f);
        return noiseValue >= min && noiseValue <= max;
    }
}
