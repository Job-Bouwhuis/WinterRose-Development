using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WinterRose.Monogame.Worlds;

/// <summary>
/// A class that contains all the options for rendering a world when the world has no camera.
/// </summary>
public sealed class WorldRenderingOptions
{
    /// <summary>
    /// The sort mode for the sprite batch. Defaults to <see cref="SpriteSortMode.FrontToBack"/>
    /// </summary>
    public SpriteSortMode SortMode { get; set; } = SpriteSortMode.FrontToBack;
    /// <summary>
    /// The blend state for the sprite batch. Defaults tp <see langword="null"/>
    /// </summary>
    public BlendState? BlendState { get; set; } = null;
    /// <summary>
    /// The sampler state for the sprite batch. Defaults to <see cref="SamplerState.PointClamp"/>
    /// </summary>
    public SamplerState? SamplerState { get; set; } = SamplerState.PointClamp;
    /// <summary>
    /// The depth stencil state for the sprite batch. Defaults to <see cref="DepthStencilState.Default"/>
    /// </summary>
    public DepthStencilState? DepthStencilState { get; set; } = DepthStencilState.Default;
    /// <summary>
    /// The rasterizer state for the sprite batch. Defaults to <see cref="RasterizerState.CullCounterClockwise"/>
    /// </summary>
    public RasterizerState? RasterizerState { get; set; } = RasterizerState.CullCounterClockwise;
    /// <summary>
    /// The effect for the sprite batch. Defaults to <see langword="null"/>
    /// </summary>
    public Effect? Effect { get; set; } = null;
    public Color ClearColor { get; set; } = Color.Black;
}
