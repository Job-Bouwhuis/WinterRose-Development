using PuppeteerSharp.Cdp;
using System.Linq.Expressions;
using System.Reflection;
using WinterRose.Reflection;

namespace WinterRose.ForgeWarden.Tweens;

public class TweenAction<TTarget, TValue> : ITweenAction
{
    private TTarget tar;
    private StructReference reference;

    public TTarget Target { get; }
    public MemberData Member { get; }
    public TValue StartValue { get; }
    public TValue EndValue { get; }
    public float Duration { get; }
    public Curve? Curve { get; }

    private float elapsed = 0f;

    public bool Completed => elapsed >= Duration;

    public TweenAction(
        ref TTarget target,
        Expression<Func<TTarget, TValue>> propertyExpression,
        TValue endValue,
        float duration,
        Curve? curve = null
    )
    {
        Target = target;
        
        if(typeof(TTarget).IsValueType)
            unsafe
            {
                fixed (TTarget* ptr = &target)
                {
                    reference = new(ptr);
                }
            }
        
        Duration = duration;
        Curve = curve;
        EndValue = endValue;

        if (propertyExpression.Body is not MemberExpression memberExpr)
            throw new ArgumentException("Expression must point to a property or field");

        if (memberExpr.Member is not PropertyInfo and not FieldInfo)
            throw new ArgumentException("Expression must point to a property or field");

        Member = MemberData.FromMemberInfo(memberExpr.Member);
        if (reference is not null)
        {
            StartValue = (TValue)Member.GetValue(ref reference.As<TTarget>());
        }
        else
            StartValue = (TValue)Member.GetValue(Target)!;
    }

    public void Update()
    {
        if (Completed) return;

        elapsed += Time.deltaTime;
        float t = Math.Clamp(elapsed / Duration, 0, 1);

        TValue value = Tweener.Tween(StartValue, EndValue, t, Curve);

        if(reference is not null)
        {
            Member.SetValue(ref reference.As<TTarget>(), value);
        }
        else
            Member.SetValue(Target, value);
    }
}

public class TweenSequence<TTarget, TValue> : TweenAction<TTarget, TValue>
{
    private readonly Queue<ITweenAction> actions = new();
    private ITweenAction? current;

    public TweenSequence(ref TTarget target, 
        Expression<Func<TTarget, TValue>> propertyExpression, 
        TValue endValue, 
        float duration, 
        Curve? curve = null) 
        : base(ref target, propertyExpression, endValue, duration, curve)
    {
    }

    public bool Completed => current == null;

    public TweenSequence<TTarget, TValue> Then<TNewTarget, TNewValue>(
        ref TNewTarget target,
        Expression<Func<TNewTarget, TNewValue>> propertyExpression,
        TNewValue endValue,
        float duration,
        Curve? curve = null)
    {
        actions.Enqueue(new TweenAction<TNewTarget, TNewValue>(
            ref target,
            propertyExpression,
            endValue,
            duration,
            curve));
        return this;
    }

    public void Update()
    {
        if (current == null && actions.Count > 0)
            current = actions.Dequeue();

        if (current == null) return;

        current.Update();

        if (current.Completed)
            current = null;
    }
}
