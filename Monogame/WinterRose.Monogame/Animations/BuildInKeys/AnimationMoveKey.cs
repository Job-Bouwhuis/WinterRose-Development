using Microsoft.Xna.Framework;
using System;
using WinterRose.Serialization;

namespace WinterRose.Monogame.Animations;

/// <summary>
/// A <see cref="AnimationKey"/> that moves a <see cref="Transform"/> to a specified position over the course of the animation
/// </summary>
public sealed class AnimationMoveKey : AnimationKey
{
    /// <summary>
    /// Creates a new <see cref="AnimationMoveKey"/> that moves the <see cref="Transform"/> to <paramref name="end"/> over the course of the animation
    /// </summary>
    /// <param name="name"></param>
    /// <param name="end"></param>
    public AnimationMoveKey(string name, Vector2 end) : base(name) => End = end;
    private AnimationMoveKey() : base("") { } // for serialization

    /// <summary>
    /// The position to move the <see cref="Transform"/> from
    /// </summary>
    [ExcludeFromSerialization]
    public Vector2 Start;
    /// <summary>
    /// The position to move the <see cref="Transform"/> to
    /// </summary>
    public Vector2 End;
    private float currentTime = 0;
    private float lerpProgress;

    Transform transform => Target as Transform;

    /// <summary>
    /// Moves the <see cref="Transform"/> to <see cref="End"/> over the course of the animation
    /// </summary>
    public override void KeyStep()
    {
        // smoothly move to end without using Vector2.Lerp
        currentTime += Time.deltaTime;
        if (currentTime is 0)
            return;
        ;
        lerpProgress += Time.deltaTime / Animation.AnimationLength;
        transform.position = Vector2.Lerp(Start, End, Math.Clamp(lerpProgress, 0, 1));
    }

    /// <summary>
    /// Cancels the animation and moves the <see cref="Transform"/> back to <see cref="Start"/>
    /// </summary>
    public override void KeyCancel() => transform.position = Start;
    /// <summary>
    /// Ends the animation and moves the <see cref="Transform"/> to <see cref="End"/>
    /// </summary>
    public override void KeyEnd() => transform.position = End;
    /// <summary>
    /// Returns true if the <see cref="Transform"/> is at <see cref="End"/> and this the key is finished
    /// </summary>
    /// <returns></returns>
    public override bool EvaluateEnd() => Vector2.Distance(transform.position, End) < 0.1f || lerpProgress >= 1f;
    /// <summary>
    /// Returns true if the <see cref="AnimationKey.Target"/> is a <see cref="Transform"/>
    /// </summary>
    /// <returns></returns>
    public override bool ValidateTarget() => Target is Transform;
    /// <summary>
    /// Sets the <see cref="AnimationKey.Target"/> to the <see cref="Transform"/> of the <paramref name="owner"/>
    /// </summary>
    /// <param name="owner"></param>
    public override void Setup(WorldObject owner) => Target = owner.transform;
    /// <summary>
    /// Sets <see cref="Start"/> to the current position of the <see cref="Transform"/>
    /// </summary>
    public override void StartKey()
    {
        lerpProgress = currentTime = 0;
        Start = transform.position;
    }
}
