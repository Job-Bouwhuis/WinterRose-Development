using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Reflection;

internal static class MemberAccessFactory
{
    public static Func<object, object> CreateGetter(MemberData member) => member.MemberType switch
    {
        MemberTypes.Field => CreateFieldGetter(member),
        MemberTypes.Property => CreatePropertyGetter(member),
        // shouldnt be reached but stiill
        _ => throw new ArgumentException("Member must be field or property"),
    };

    public static Action<object, object> CreateSetter(MemberData member) => member.MemberType switch
    {
        MemberTypes.Field => CreateFieldSetter(member),
        MemberTypes.Property => CreatePropertySetter(member),
        // shouldnt be reached but stiill
        _ => throw new ArgumentException("Member must be field or property"),
    };

    private static Func<object, object> CreateFieldGetter(FieldInfo field)
    {
        var instanceParam = Expression.Parameter(typeof(object), "instance");

        Expression fieldAccess = field.IsStatic
            ? Expression.Field(null, field)
            : Expression.Field(Expression.Convert(instanceParam, field.DeclaringType), field);

        var castResult = Expression.Convert(fieldAccess, typeof(object));
        var lambda = Expression.Lambda<Func<object, object>>(castResult, instanceParam);
        return lambda.Compile();
    }

    private static Action<object, object>? CreateFieldSetter(FieldInfo field)
    {
        if (field.IsInitOnly) 
            return null;

        var instanceParam = Expression.Parameter(typeof(object), "instance");
        var valueParam = Expression.Parameter(typeof(object), "value");
        var castValue = Expression.Convert(valueParam, field.FieldType);

        Expression fieldAccess = field.IsStatic
            ? Expression.Field(null, field)
            : Expression.Field(Expression.Convert(instanceParam, field.DeclaringType), field);

        var assign = Expression.Assign(fieldAccess, castValue);
        var lambda = Expression.Lambda<Action<object, object>>(assign, instanceParam, valueParam);
        return lambda.Compile();
    }

    private static Func<object, object> CreatePropertyGetter(PropertyInfo prop)
    {
        var getter = prop.GetGetMethod(true)
            ?? throw new InvalidOperationException("Property has no getter");

        var instanceParam = Expression.Parameter(typeof(object), "instance");

        Expression callGetter = getter.IsStatic
            ? Expression.Call(getter)
            : Expression.Call(Expression.Convert(instanceParam, prop.DeclaringType), getter);

        var castResult = Expression.Convert(callGetter, typeof(object));
        var lambda = Expression.Lambda<Func<object, object>>(castResult, instanceParam);
        return lambda.Compile();
    }

    private static Action<object, object>? CreatePropertySetter(PropertyInfo prop)
    {
        var setter = prop.GetSetMethod(true);
        if (setter == null) return null;

        var instanceParam = Expression.Parameter(typeof(object), "instance");
        var valueParam = Expression.Parameter(typeof(object), "value");
        var castValue = Expression.Convert(valueParam, prop.PropertyType);

        Expression callSetter = setter.IsStatic
            ? Expression.Call(setter, castValue)
            : Expression.Call(Expression.Convert(instanceParam, prop.DeclaringType), setter, castValue);

        var lambda = Expression.Lambda<Action<object, object>>(callSetter, instanceParam, valueParam);
        return lambda.Compile();
    }

}
