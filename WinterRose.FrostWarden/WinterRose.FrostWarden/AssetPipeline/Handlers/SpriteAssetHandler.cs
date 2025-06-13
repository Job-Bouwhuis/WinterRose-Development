using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.AnonymousTypes;

namespace WinterRose.FrostWarden.AssetPipeline.Handlers
{
    internal sealed class SpriteAssetHandler : IAssetHandler<Sprite>
    {
        public static string[] InterestedInExtensions => [".png", ".jpg", ".gif"];

        public static bool InitializeNewAsset(AssetHeader header)
        {
            if(Path.GetExtension(header.Path) == ".gif")
            {
                header.Metadata ??= new Anonymous();
                header.Metadata["type"] = "gif";
            }
            return true;
        }

        public static Sprite LoadAsset(AssetHeader header)
        {
            if ((header.Metadata?.TryGet("type", out string? type) ?? false) && type == "gif")
                return new SpriteGif(header.Path);
            return new Sprite(header.Path);
        }
        public static bool SaveAsset(AssetHeader header, Sprite asset)
        {
            Image img = ray.LoadImageFromTexture(asset.Texture);
            ray.ExportImage(img, header.Path);
            ray.UnloadImage(img);

            if(asset is SpriteGif)
            {
                header.Metadata["type"] = "gif";
            }

            return true;

        }
    }
}
