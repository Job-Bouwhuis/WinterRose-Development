using WinterRose.EventBusses;
using WinterRose.ForgeWarden.TextRendering;

namespace WinterRose.ForgeWarden.UserInterface;

public abstract class UINumericControlBase<T> : UIContent, INumericControl<T>
    where T : INumber<T>, IMinMaxValue<T>
{
    public String Label { get; set; }
    public T MinValue { get; set; } = T.MinValue;
    public T MaxValue { get; set; } = T.MaxValue;
    public T Step { get; set; } = T.Zero;

    public MulticastVoidInvocation<T> TypedValueChanged { get; } = new();
    public MulticastVoidInvocation<double> ValueChanged { get; } = new();

    protected T valueBacking { get; private set; }

    public virtual T Value
    {
        get => valueBacking;
        set => SetValue(value, invokeCallbacks: true);
    }

    // INumericControl double facade
    public double ValueAsDouble
    {
        get => Convert.ToDouble(valueBacking);
        set
        {
            try
            {
                var t = (T)Convert.ChangeType(value, typeof(T));
                Value = t;
            }
            catch { /* ignore conversion errors */ }
        }
    }

    public double MinAsDouble => Convert.ToDouble(MinValue);
    public double MaxAsDouble => Convert.ToDouble(MaxValue);
    public double StepAsDouble => Convert.ToDouble(Step);

    public virtual void SetValue(T newVal, bool invokeCallbacks)
    {
        // clamp
        if (MinValue.CompareTo(MaxValue) > 0)
        {
            var tmp = MinValue;
            MinValue = MaxValue;
            MaxValue = tmp;
        }

        var clamped = Clamp(newVal, MinValue, MaxValue);
        bool changed = !EqualityComparer<T>.Default.Equals(valueBacking, clamped);
        valueBacking = clamped;

        if (changed && invokeCallbacks)
        {
            TypedValueChanged?.Invoke(valueBacking);
            ValueChanged?.Invoke(Convert.ToDouble(valueBacking));
        }
    }

    private static T Clamp(T v, T minValue, T maxValue)
    {
        if (v < minValue) return minValue;
        if (v > maxValue) return maxValue;
        return v;
    }
}