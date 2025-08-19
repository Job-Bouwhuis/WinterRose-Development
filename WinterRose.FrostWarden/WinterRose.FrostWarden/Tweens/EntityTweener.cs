using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.Tweens;

public class EntityTweener : Component, IUpdatable
{
    private readonly List<ITweenAction> tweens = new();

    public void Add<TTarget, TValue>(TweenAction<TTarget, TValue> tween)
    {
        tweens.Add(tween);
    }

    public void Add<TTarget, TValue>(
        TTarget target,
        Expression<Func<TTarget, TValue>> propertyExpression,
        TValue endValue,
        float duration,
        Curve? curve = null) where TTarget : class
        => Add<TTarget, TValue>(new(ref target, propertyExpression, endValue, duration, curve));

    public void Add<TTarget, TValue>(
    ref TTarget target,
    Expression<Func<TTarget, TValue>> propertyExpression,
    TValue endValue,
    float duration,
    Curve? curve = null) where TTarget : struct
    => Add<TTarget, TValue>(new(ref target, propertyExpression, endValue, duration, curve));

    public TweenSequence<TTarget, TValue> Sequence<TTarget, TValue>(
    TTarget target,
    Expression<Func<TTarget, TValue>> propertyExpression,
    TValue endValue,
    float duration,
    Curve? curve = null) where TTarget : class
    {
        var seq = new TweenSequence<TTarget, TValue>(ref target, propertyExpression, endValue, duration, curve);
        Add(seq);
        return seq;
    }

    public TweenSequence<TTarget, TValue> Sequence<TTarget, TValue>(
    ref TTarget target,
    Expression<Func<TTarget, TValue>> propertyExpression,
    TValue endValue,
    float duration,
    Curve? curve = null) where TTarget : struct
    {
        var seq = new TweenSequence<TTarget, TValue>(ref target, propertyExpression, endValue, duration, curve);
        Add(seq);
        return seq;
    }

    public void Update()
    {
        for (int i = tweens.Count - 1; i >= 0; i--)
        {
            var tween = tweens[i];
            tween.Update();

            if (tween.Completed)
                tweens.RemoveAt(i);
        }
    }
}