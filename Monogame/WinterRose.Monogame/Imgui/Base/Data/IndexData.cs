using Microsoft.Xna.Framework.Graphics;

namespace WinterRose.Monogame.Imgui.Data;

/// <summary>
///     Contains information regarding the index buffer used by the GUIRenderer.
/// </summary>
public class IndexData
{
    public IndexBuffer Buffer;
    public int BufferSize;
    public byte[] Data;
}