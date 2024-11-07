using Microsoft.Xna.Framework;
using WinterRose.Monogame;

namespace WinterRose.Monogame.Interfaces;

/// <summary>
/// Allows for a <see cref="ObjectComponent"/> or <see cref="ObjectBehavior"/> to know when the transform's position changed.<br></br>
/// using <see cref="OnAfterPositionChanged"/> and <see cref="OnBeforePositionChanged"/>
/// </summary>
public interface IPositionChangedCallback
{
    public virtual void OnBeforePositionChanged(Vector2 newPosition) { }
    public void OnAfterPositionChanged() { }
}
