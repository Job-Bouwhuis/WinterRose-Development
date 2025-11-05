
namespace WinterRose.ForgeWarden.AssetPipeline;

[Serializable]
internal class AssetException : Exception
{
    public AssetException(string? message) : base(message)
    {
    }
}