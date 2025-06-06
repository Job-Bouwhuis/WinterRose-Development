using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Reflection;

namespace WinterRose;

public class DynamicTypeProvider<T> : DynamicObject where T : new()
{
    private Dictionary<string, object?> fields = new();
    private Dictionary<string, object?> properties = new();

    public static implicit operator T(DynamicTypeProvider<T> provider) => provider.GetObject();
    public static implicit operator DynamicTypeProvider<T>(T obj) => new(obj);

    public DynamicTypeProvider()
    {
    }   

    public DynamicTypeProvider(T obj)
    {
        foreach (var field in typeof(T).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            fields[field.Name] = field.GetValue(obj);
        foreach (var property in typeof(T).GetProperties(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            properties[property.Name] = property.GetValue(obj);
    }

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        if (fields.ContainsKey(binder.Name))
        {
            result = fields[binder.Name];
            return true;
        }
        else
        {
            result = null;
            return false;
        }
    }

    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        fields[binder.Name] = value;
        return true;
    }

    public T GetObject()
    {
        object obj = new T();
        ReflectionHelper rh = ReflectionHelper.ForObject(ref obj);

        foreach (var field in fields)
            rh.SetValue(field.Key, field.Value);

        return (T)obj;
    }

    public dynamic this[string index]
    {
        get => fields[index];
        set => fields[index] = value;
    }

}
