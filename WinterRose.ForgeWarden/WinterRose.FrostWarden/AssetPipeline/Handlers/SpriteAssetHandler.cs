using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.AnonymousTypes;
using WinterRose.ForgeWarden.AssetPipeline;

namespace WinterRose.ForgeWarden.AssetPipeline.Handlers
{
    internal sealed class SpriteAssetHandler : IAssetHandler<Sprite>, IAssetHandler<SpriteGif>
    {
        public static string[] InterestedInExtensions => [".png", ".jpg", ".gif"];

        public static bool InitializeNewAsset(AssetHeader header)
        {
            if(Path.GetExtension(header.Path) == ".gif")
            {
                header.Metadata["type"] = "gif";
            }
            return true;
        }

        public static Sprite LoadAsset(AssetHeader header)
        {
            if ((header.Metadata?.TryGet("type", out string? type) ?? false) && type == "gif")
                return new SpriteGif(header.Source.Name)
                {
                    Source = header.Name
                };
            return new Sprite(header.Source.Name)
            {
                Source = header.Name
            };
        }
        public static bool SaveAsset(AssetHeader header, Sprite asset)
        {
            if (string.IsNullOrWhiteSpace(header.Path))
                throw new InvalidOperationException("This sprite can not be saved!");

            Image img = ray.LoadImageFromTexture(asset.Texture);

            if(asset is SpriteGif)
            {
                header.Metadata["type"] = "gif";
                throw new NotImplementedException("Saving gifs is not implemented yet!");
            }
            else
            {
                ray.ExportImage(img, header.Path);
                ray.UnloadImage(img);
            }

            return true;

        }

        public static bool SaveAsset(AssetHeader header, SpriteGif asset) => SaveAsset(header, asset);
        public static bool SaveAsset(string assetName, Sprite asset) => SaveAsset(Assets.GetHeader(assetName), asset);
        public static bool SaveAsset(string assetName, SpriteGif asset) => SaveAsset(Assets.GetHeader(assetName), asset);
        static SpriteGif IAssetHandler<SpriteGif>.LoadAsset(AssetHeader header) => LoadAsset(header) as SpriteGif 
            ?? throw new Exception("Given sprite was not a gif");
    }
}