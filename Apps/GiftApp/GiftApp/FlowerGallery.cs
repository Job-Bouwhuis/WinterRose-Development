using Raylib_cs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using WinterRose;
using WinterRose.EventBusses;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.AssetPipeline;
using WinterRose.ForgeWarden.Geometry;
using WinterRose.ForgeWarden.Geometry.Rendering;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.UserInterface;
using WinterRose.ForgeWarden.UserInterface.Content;
using WinterRose.ForgeWarden.UserInterface.Windowing;
using WinterRose.WinterForgeSerializing;
using static WinterRose.ForgeWarden.Geometry.GeometricFlowerBuilder;

namespace GiftApp;

public sealed class GeneratedFlowerAsset
{
    public FlowerConfig Config { get; set; } = new FlowerConfig();
    public string ThumbnailPath { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public ShapeCollection Shapes { get; internal set; }
}

public sealed class FlowerFavoritesAsset
{
    public List<string> FavoriteFlowerNames { get; set; } = [];
}

public sealed class GeneratedFlowerAssetHandler : IAssetHandler<GeneratedFlowerAsset>
{
    public static string[] InterestedInExtensions => [".fwasset"];

    public static bool SaveAsset(AssetHeader header, GeneratedFlowerAsset asset)
    {
        EnsureThumbnailDirectory(asset.ThumbnailPath);

        if (string.IsNullOrWhiteSpace(asset.ThumbnailPath))
            asset.ThumbnailPath = GetDefaultThumbnailPath(header.Name);

        if (!File.Exists(asset.ThumbnailPath) && asset.Config is not null)
        {
            byte[] thumbnail = FlowerGalleryManager.CreateThumbnailPng(asset.Shapes);
            File.WriteAllBytes(asset.ThumbnailPath, thumbnail);
        }

        WinterForge.SerializeToFile(asset, header.Path);
        return true;
    }

    public static bool SaveAsset(string name, GeneratedFlowerAsset asset)
    {
        return SaveAsset(Assets.GetHeader(name), asset);
    }

    public static GeneratedFlowerAsset LoadAsset(AssetHeader header)
    {
        if (!File.Exists(header.Path))
            return new GeneratedFlowerAsset
            {
                ThumbnailPath = GetDefaultThumbnailPath(header.Name)
            };

        GeneratedFlowerAsset asset = WinterForge.DeserializeFromFile<GeneratedFlowerAsset>(header.Path);

        if (string.IsNullOrWhiteSpace(asset.ThumbnailPath))
            asset.ThumbnailPath = GetDefaultThumbnailPath(header.Name);

        return asset;
    }

    public static bool InitializeNewAsset(AssetHeader header)
    {
        return SaveAsset(header, new GeneratedFlowerAsset
        {
            ThumbnailPath = GetDefaultThumbnailPath(header.Name)
        });
    }

    private static string GetDefaultThumbnailPath(string assetName)
    {
        return Path.Combine("Assets", "FlowerThumbnails", assetName + ".png");
    }

    private static void EnsureThumbnailDirectory(string thumbnailPath)
    {
        if (string.IsNullOrWhiteSpace(thumbnailPath))
            return;

        string? directory = Path.GetDirectoryName(thumbnailPath);
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);
    }
}

public sealed class FlowerFavoritesAssetHandler : IAssetHandler<FlowerFavoritesAsset>
{
    public static string[] InterestedInExtensions => [".fwasset"];

    public static bool SaveAsset(AssetHeader header, FlowerFavoritesAsset asset)
    {
        WinterForge.SerializeToFile(asset, header.Path);
        return true;
    }

    public static bool SaveAsset(string name, FlowerFavoritesAsset asset)
    {
        return SaveAsset(Assets.GetHeader(name), asset);
    }

    public static FlowerFavoritesAsset LoadAsset(AssetHeader header)
    {
        if (!File.Exists(header.Path))
            return new FlowerFavoritesAsset();

        return WinterForge.DeserializeFromFile<FlowerFavoritesAsset>(header.Path);
    }

    public static bool InitializeNewAsset(AssetHeader header)
    {
        return SaveAsset(header, new FlowerFavoritesAsset());
    }
}

public sealed record GeneratedFlowerEntry(
    string AssetName,
    GeneratedFlowerAsset Asset
)
{
    public bool IsFavorite => FlowerGalleryManager.IsFavorite(AssetName);
}

public static class FlowerGalleryManager
{
    private const string FAVORITES_ASSET_NAME = "FlowerGalleryFavorites";
    private const int THUMBNAIL_SIZE = 256;
    private const int THUMBNAIL_PADDING = 18;

    private static readonly List<GeneratedFlowerEntry> SESSION_FLOWERS = [];
    private static readonly HashSet<string> FAVORITE_NAMES = new(StringComparer.OrdinalIgnoreCase);

    private static FlowerFavoritesAsset favoritesAsset = new();
    private static bool isInitialized;

    public static void Initialize()
    {
        if (isInitialized)
            return;

        SESSION_FLOWERS.Clear();
        FAVORITE_NAMES.Clear();

        if (!Assets.Exists(FAVORITES_ASSET_NAME))
            Assets.CreateAsset<FlowerFavoritesAsset>(FAVORITES_ASSET_NAME);

        favoritesAsset = Assets.Load<FlowerFavoritesAsset>(FAVORITES_ASSET_NAME) ?? new FlowerFavoritesAsset();

        foreach (string name in favoritesAsset.FavoriteFlowerNames)
            FAVORITE_NAMES.Add(name);

        isInitialized = true;
    }

    public static void Shutdown()
    {
        if (!isInitialized)
            return;

        SyncFavoritesAsset();
        Assets.Save(FAVORITES_ASSET_NAME, favoritesAsset);

        var thumbnails = new DirectoryInfo("FlowerThumbnails").EnumerateFiles();

        foreach(var t in thumbnails)
            if(!favoritesAsset.FavoriteFlowerNames.Contains(Path.GetFileNameWithoutExtension(t.FullName)))
                t.Delete();
    }

    public static void ClearSession()
    {
        SESSION_FLOWERS.Clear();
    }

    public static GeneratedFlowerEntry RegisterGeneratedFlower(FlowerConfig config, ShapeCollection shapes)
    {
        Initialize();

        string assetName = GenerateFlowerAssetName();
        string thumbnailPath = Path.Combine("FlowerThumbnails", assetName + ".png");

        if (!Directory.Exists(Path.GetDirectoryName(thumbnailPath)!))
            Directory.CreateDirectory(Path.GetDirectoryName(thumbnailPath)!);

        byte[] thumbnailPng = CreateThumbnailPng(shapes);
        File.WriteAllBytes(thumbnailPath, thumbnailPng);

        GeneratedFlowerAsset asset = new GeneratedFlowerAsset
        {
            Config = config,
            Shapes = shapes,
            ThumbnailPath = thumbnailPath,
            CreatedUtc = DateTime.UtcNow
        };

        GeneratedFlowerEntry entry = new GeneratedFlowerEntry(assetName, asset);

        SESSION_FLOWERS.Add(entry);

        return entry;
    }

    public static IReadOnlyList<GeneratedFlowerEntry> GetSessionFlowers()
    {
        Initialize();
        return SESSION_FLOWERS;
    }

    public static IReadOnlyList<GeneratedFlowerEntry> GetFavoriteFlowers()
    {
        Initialize();

        List<GeneratedFlowerEntry> result = new();

        foreach (string name in FAVORITE_NAMES.OrderBy(x => x))
        {
            if (!Assets.Exists(name))
                continue;

            GeneratedFlowerAsset? asset = Assets.Load<GeneratedFlowerAsset>(name);
            if (asset is null)
                continue;

            result.Add(new GeneratedFlowerEntry(name, asset));
        }

        return result;
    }

    public static bool IsFavorite(string flowerAssetName)
    {
        Initialize();
        return FAVORITE_NAMES.Contains(flowerAssetName);
    }

    public static void SetFavorite(string flowerAssetName, bool favorite)
    {
        Initialize();

        if (favorite)
        {
            FAVORITE_NAMES.Add(flowerAssetName);

            GeneratedFlowerEntry? sessionEntry = SESSION_FLOWERS
                .FirstOrDefault(x => x.AssetName == flowerAssetName);

            if (sessionEntry is not null)
            {
                Assets.CreateAsset(sessionEntry.Asset, flowerAssetName);
            }
        }
        else
        {
            FAVORITE_NAMES.Remove(flowerAssetName);

            Assets.Delete(flowerAssetName);
        }

        SyncFavoritesAsset();
        Assets.Save(FAVORITES_ASSET_NAME, favoritesAsset);
    }

    public static void ToggleFavorite(string flowerAssetName)
    {
        SetFavorite(flowerAssetName, !IsFavorite(flowerAssetName));
    }

    public static UIWindow CreateFlowerGalleryWindow()
    {
        Initialize();

        UIWindow window = new UIWindow("Flower Gallery", 1550, 900);
        window.Style.ShowVerticalScrollBar = true;

        UIColumns root = new UIColumns();
        window.AddContent(root);

        UIColumns sessionColumn = new UIColumns();
        UIColumns favoriteColumn = new UIColumns();

        root.AddToColumn(0, sessionColumn);
        root.AddToColumn(1, favoriteColumn);

        Action rebuild = null!;
        rebuild = () =>
        {
            sessionColumn.ClearColumn(0);
            sessionColumn.ClearColumn(1);
            favoriteColumn.ClearColumn(0);
            favoriteColumn.ClearColumn(1);

            sessionColumn.AddToColumn(0, new UIText("Generated this session", UIFontSizePreset.Title));
            favoriteColumn.AddToColumn(0, new UIText("Favorites", UIFontSizePreset.Title));

            List<GeneratedFlowerEntry> sessionFlowers = GetSessionFlowers()
                .OrderByDescending(x => x.Asset.CreatedUtc)
                .ToList();

            List<GeneratedFlowerEntry> favoriteFlowers = GetFavoriteFlowers()
                .OrderByDescending(x => x.Asset.CreatedUtc)
                .ToList();

            if (sessionFlowers.Count == 0)
                sessionColumn.AddToColumn(0, new UIText("No flowers generated yet.", UIFontSizePreset.Text));

            if (favoriteFlowers.Count == 0)
                favoriteColumn.AddToColumn(0, new UIText("No favorites yet.", UIFontSizePreset.Text));

            foreach (GeneratedFlowerEntry entry in sessionFlowers)
                sessionColumn.AddToColumn(0, BuildFlowerCard(entry, rebuild));

            foreach (GeneratedFlowerEntry entry in favoriteFlowers)
                favoriteColumn.AddToColumn(0, BuildFlowerCard(entry, rebuild));
        };

        rebuild();
        return window;
    }

    public static UIWindow CreateFlowerOnlyWindow(GeneratedFlowerEntry entry)
    {
        UIWindow window = new UIWindow("Flower Viewer", 800, 800);
        UISprite sprite = new UISprite(entry.Asset.ThumbnailPath)
        {
            MaxHeight = 1000,
            DisposeSpriteOnOwnerClose = false
        };
        window.AddContent(sprite);
        return window;
    }

    private static UIContent BuildFlowerCard(GeneratedFlowerEntry entry, Action rebuild)
    {
        UIColumns card = new UIColumns();
        card.ColumnScrollEnabled[0] = false;
        card.ColumnScrollEnabled[1] = false;

        UISprite sprite = new UISprite(entry.Asset.ThumbnailPath)
        {
            DisposeSpriteOnOwnerClose = false
        };

        card.AddToColumn(0, sprite);

        UIColumns info = new UIColumns();
        info.ColumnScrollEnabled[0] = false;

        //info.AddToColumn(0, new UIText(entry.AssetName, UIFontSizePreset.Subtitle));
        info.AddToColumn(0, new UIText($"Created: {entry.Asset.CreatedUtc.ToLocalTime():yyyy-MM-dd HH:mm:ss}", UIFontSizePreset.Text));

        UIButton favoriteButton = new UIButton(
            entry.IsFavorite ? "Unfavorite" : "Favorite",
            (owner, button) =>
            {
                ToggleFavorite(entry.AssetName);
                rebuild();
            });

        info.AddToColumn(0, favoriteButton);

        UIButton OpenFull = new UIButton(
            "Open in seperate window",
            (owner, button) =>
            {
                ((UIWindow)owner.Root).Collapsed = true;
                UIWindow w = CreateFlowerOnlyWindow(entry);
                w.ShowMaximized();
                w.OnClosing.Subscribe(Invocation.Create((UIWindow w) =>
                {
                    ((UIWindow)owner.Root).Collapsed = false;
                }));


            });

        info.AddToColumn(0, OpenFull);

        card.AddToColumn(1, info);
        return card;
    }

    private static void SyncFavoritesAsset()
    {
        favoritesAsset.FavoriteFlowerNames = FAVORITE_NAMES.OrderBy(x => x).ToList();
    }

    private static string GenerateFlowerAssetName()
    {
        long ticks = DateTime.UtcNow.Ticks;
        int randomValue = Raylib.GetRandomValue(1000, 9999);
        return $"flower{ticks}{randomValue}";
    }

    public static byte[] CreateThumbnailPng(ShapeCollection flower)
    {
        var shapes = flower.Duplicate();
        shapes.EnsureAnimationEnd(shapes);

        RenderTexture2D target = Raylib.LoadRenderTexture(THUMBNAIL_SIZE, THUMBNAIL_SIZE);

        ShapeRenderer renderer = new(new ShapeAnimationSystem());

        try
        {
            List<ShapeSnapshot> snapshots = shapes.GetSnapshot().ToList();

            if (snapshots.Count > 0)
            {
                float minX = float.MaxValue;
                float minY = float.MaxValue;
                float maxX = float.MinValue;
                float maxY = float.MinValue;

                foreach (ShapeSnapshot snapshot in snapshots)
                {
                    foreach (Vector2 point in snapshot.Points)
                    {
                        if (point.X < minX) 
                            minX = point.X;
                        if (point.Y < minY) 
                            minY = point.Y;
                        if (point.X > maxX) 
                            maxX = point.X;
                        if (point.Y > maxY) 
                            maxY = point.Y;
                    }
                }

                float width = Math.Max(1f, maxX - minX);
                float height = Math.Max(1f, maxY - minY);
                float available = THUMBNAIL_SIZE - (THUMBNAIL_PADDING * 2);
                float scale = Math.Min(available / width, available / height);

                if (float.IsNaN(scale) || float.IsInfinity(scale) || scale <= 0f)
                    scale = 1f;

                Vector2 targetCenter = new Vector2(THUMBNAIL_SIZE * 0.5f, THUMBNAIL_SIZE * 0.5f);
                Vector2 sourceCenter = shapes.GetAverageCenter();

                foreach (ShapeSnapshot snapshot in snapshots.OrderBy(x => x.Layer))
                {
                    Vector2[] points = new Vector2[snapshot.Points.Length];

                    for (int i = 0; i < snapshot.Points.Length; i++)
                    {
                        Vector2 local = snapshot.Points[i] - sourceCenter;

                        local *= scale;

                        points[i] = targetCenter + local;
                    }

                    ShapePath previewPath = new ShapePath(points, snapshot.Closed, snapshot.Style, snapshot.Layer);
                    renderer.DrawPath(previewPath);
                }
            }
            
            Raylib.BeginTextureMode(target);
            Raylib.ClearBackground(Color.Blank);

            renderer.Draw();

            Raylib.EndTextureMode();

            // flip fix happens here
            RenderTexture2D flipped = target;

            Image image = Raylib.LoadImageFromTexture(flipped.Texture);
            Raylib.ImageFlipVertical(ref image);

            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".png");
            Raylib.ExportImage(image, tempPath);
            Raylib.UnloadImage(image);

            byte[] bytes = File.ReadAllBytes(tempPath);

            try
            {
                File.Delete(tempPath);
            }
            catch
            {
            }

            return bytes;
        }
        finally
        {
            Raylib.UnloadRenderTexture(target);
        }
    }
}