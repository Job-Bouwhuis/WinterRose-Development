using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.Animations;

/// <summary>
/// A key that rotates a <see cref="Transform"/> to a specified rotation
/// </summary>
public sealed class AnimationRotateKey : AnimationKey
{
    float startRotation;
    /// <summary>
    /// The rotation to rotate to
    /// </summary>
    public float endRotation;

    Transform transform => Target as Transform;

    /// <summary>
    /// Creates a new <see cref="AnimationRotateKey"/> with the specified name and end rotation
    /// </summary>
    /// <param name="name">the name of this key, can be used to index it</param>
    /// <param name="endRotation">the rotation this key will rotate the transform to</param>
    public AnimationRotateKey(string name, float endRotation) : base(name)
    {
        this.endRotation = endRotation;
    }
    private AnimationRotateKey() : base("") { } // For serialization

    /// <summary>
    /// Checks if the current rotation is approximately equal to the end rotation
    /// </summary>
    /// <returns></returns>
    public override bool EvaluateEnd()
    {
        // Check if the current rotation is approximately equal to the end rotation
        if(endRotation > 0)
            return transform.rotation >= endRotation;
        else
            return transform.rotation <= endRotation;
    }

    /// <summary>
    /// Cancels to rotation by setting it back to the start rotation
    /// </summary>
    public override void KeyCancel()
    {
        transform.rotation = startRotation;
    }

    /// <summary>
    /// Sets the rotation to the end rotation
    /// </summary>
    public override void KeyEnd()
    {
        transform.rotation = endRotation;
    }

    /// <summary>
    /// Rotates the transform to the end rotation smoothly
    /// </summary>
    public override void KeyStep()
    {
        // Lerp the rotation from the current to the end
        transform.rotation = MathHelper.Lerp(startRotation, endRotation, Animation.AnimationTime / Animation.AnimationLength);
    }

    /// <summary>
    /// Sets the transform this key will rotate
    /// </summary>
    /// <param name="owner"></param>
    public override void Setup(WorldObject owner)
    {
        Target = owner.transform;
    }

    /// <summary>
    /// Sets the start rotation to the current rotation
    /// </summary>
    public override void StartKey()
    {
        startRotation = transform.rotation;
    }

    /// <summary>
    /// Validates that the target is a <see cref="Transform"/>
    /// </summary>
    /// <returns></returns>
    public override bool ValidateTarget() => Target is Transform;
}
