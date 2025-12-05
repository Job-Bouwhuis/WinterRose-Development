using System;
using System.Collections.Generic;
using System.Text;
using WinterRose.Reflection;

namespace WinterRose.ForgeCodex.AutoKeys;

public abstract class AutoKey
{
    internal protected abstract Type KeyType { get; }
    [WFExclude]
    public object _Key { get; set; }
}

public class Auto<T> : AutoKey where T : notnull
{
    protected internal override Type KeyType => typeof(T);

    public Auto() { }
    public Auto(T key) => Key = key;

    public new T Key { get => (T)_Key; set => _Key = value;  }
    public static implicit operator Auto<T>(T t) => new(t);

    public override string ToString() => $"Auto({Key})";
}

public class AutoToTConverter<T> : TypeConverter<Auto<T>, T>
{
    public override T Convert(Auto<T> source) => source.Key;
}
