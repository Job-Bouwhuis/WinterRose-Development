using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using WinterRose;

namespace ForgeMantle.Values;
public class BoxedConfigValue<T> : IConfigValue
{
    [WFInclude]
    public T? Value { get; private set; }

    public BoxedConfigValue(T? value)
    {
        Value = value;
    }

    private BoxedConfigValue() { } // for serialization

    public object? Get() => Value;
    public Type ValueType => typeof(T);

    public override string ToString() => Value?.ToString() ?? "null";

    public static implicit operator BoxedConfigValue<T>(T? value) => new BoxedConfigValue<T>(value);
    public static implicit operator T?(BoxedConfigValue<T> value) => value.Value;
}


