using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using WinterRose;

namespace WinterRose.ForgeMantle.Values;
public class ConfigValue<T> : IConfigValue
{
    [WFInclude]
    public T? Value { get; private set; }

    public ConfigValue(T? value)
    {
        Value = value;
    }

    private ConfigValue() { } // for serialization

    public object? Get() => Value;
    public Type ValueType => typeof(T);

    public override string ToString() => Value?.ToString() ?? "null";

    public static implicit operator ConfigValue<T>(T? value) => new ConfigValue<T>(value);
    public static implicit operator T?(ConfigValue<T> value) => value.Value;
}


