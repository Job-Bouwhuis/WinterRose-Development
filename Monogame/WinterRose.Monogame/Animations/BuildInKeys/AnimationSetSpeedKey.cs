using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.Animations;

/// <summary>
/// Sets the speed of the animation during the animation
/// </summary>
public sealed class AnimationSetSpeedKey : AnimationKey
{
    public float speed;
    public AnimationSetSpeedKey(float seconds) : base("Speed Change to " + seconds)
    {
        this.speed = seconds;
    }
    private AnimationSetSpeedKey() : base("") { } // For serialization

    /// <summary>
    /// Always returns true because this key is only used to change the speed of the animation
    /// </summary>
    /// <returns></returns>
    public override bool EvaluateEnd()
    {
        return true;
    }

    /// <summary>
    /// Not used by this key
    /// </summary>
    public override void KeyCancel()
    {
        
    }

    /// <summary>
    /// Not used by this key
    /// </summary>
    public override void KeyEnd()
    {
        
    }

    /// <summary>
    /// Sets the speed of the animation to the speed of this key
    /// </summary>
    public override void KeyStep()
    {
        Animation.AnimationLength = speed;
    }

    /// <summary>
    /// Not used by this key
    /// </summary>
    /// <param name="owner"></param>
    public override void Setup(WorldObject owner)
    {
    }

    /// <summary>
    /// Not used by this key
    /// </summary>
    public override void StartKey()
    {
    }

    /// <summary>
    /// Always returns true because this key is only used to change the speed of the animation
    /// </summary>
    /// <returns></returns>
    public override bool ValidateTarget()
    {
        return true;
    }
}
