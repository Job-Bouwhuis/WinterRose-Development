
namespace WinterRose.ForgeWarden.AssetPipeline;

[Serializable]
internal class AssetNotFoundException(string assetName) : Exception($"Asset with name '{assetName}' was not found. This can happen when its header file was not (yet) generated.");