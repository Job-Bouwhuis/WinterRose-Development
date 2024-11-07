using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.Animations;

/// <summary>
/// Animation key for scaling a <see cref="Transform"/> from one scale to another
/// </summary>
public sealed class AnimationScaleKey : AnimationKey
{
    Transform transform => Target as Transform;
    /// <summary>
    /// The end scale of the animation
    /// </summary>
    public Vector2 endScale;
    Vector2 startScale;

    public AnimationScaleKey(string name, Vector2 endScale) : base(name)
    {
        this.endScale = endScale;
    }
    private AnimationScaleKey() : base("") { } // For serialization 

    /// <summary>
    /// Evaluates if the animation has reached the end scale
    /// </summary>
    /// <returns></returns>
    public override bool EvaluateEnd()
    {
        return Math.Abs(transform.scale.X - endScale.X) < 0.01f && Math.Abs(transform.scale.Y - endScale.Y) < 0.01f;
    }

    /// <summary>
    /// Cancels the animation and sets the scale to the start scale
    /// </summary>
    public override void KeyCancel()
    {
        transform.scale = startScale;
    }

    /// <summary>
    /// Ends the animation and sets the scale to the end scale
    /// </summary>
    public override void KeyEnd()
    {
        transform.scale = endScale;
    }

    /// <summary>
    /// Steps the animation by lerping the scale from the start to the end
    /// </summary>
    public override void KeyStep()
    {
        // lerp the scale from current to end
        transform.scale = Vector2.Lerp(startScale, endScale, Animation.AnimationTime / Animation.AnimationLength);
    }

    /// <summary>
    /// Sets the Target for the animation
    /// </summary>
    /// <param name="owner"></param>
    public override void Setup(WorldObject owner)
    {
        Target = owner.transform;
    }

    /// <summary>
    /// Stars the key and sets the start scale
    /// </summary>
    public override void StartKey()
    {
        startScale = transform.scale;
    }
}
