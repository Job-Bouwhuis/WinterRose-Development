using WinterRose.ForgeSignal;

namespace WinterRose.ForgeWarden.UserInterface;

public interface INumericControl
{
    double ValueAsDouble { get; set; }
    double MinAsDouble { get; }
    double MaxAsDouble { get; }
    double StepAsDouble { get; }

    MulticastVoidInvocation<double> ValueChanged { get; }
}

public interface INumericControl<T> : INumericControl where T : INumber<T>
{
    // typed surface
    T Value { get; set; }
    T MinValue { get; set; }
    T MaxValue { get; set; }
    T Step { get; set; }

    MulticastVoidInvocation<T> TypedValueChanged  { get; }
}