using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.ForgeWarden.Geometry.Animation;

namespace WinterRose.ForgeWarden.Geometry.Rendering;

public sealed class ShapeAnimationSystem
{
    readonly List<AnimatedShape> animations = new();

    public void Add(AnimatedShape shape)
    {
        if (shape == null) throw new ArgumentNullException(nameof(shape));
        animations.Add(shape);
    }

    public void Update()
    {
        for (int i = animations.Count - 1; i >= 0; i--)
        {
            animations[i].Update();

            if (!animations[i].IsPlaying)
                animations.RemoveAt(i);
        }
    }

    internal IReadOnlyList<AnimatedShape> ActiveAnimations
        => animations;
}
