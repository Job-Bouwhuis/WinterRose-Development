using System.Collections.Generic;
using System.Runtime.CompilerServices;
using WinterRose;

namespace WinterRose.ForgeCodex.AutoKeys;

public abstract class KeyFactory
{
    private static Dictionary<Type, KeyFactory> factories = [];

    static KeyFactory()
    {
        Type[] factories = TypeWorker.FindTypesWithBase<KeyFactory>();
        foreach(var fac in factories)
        {
            if (fac == typeof(KeyFactory) || fac == typeof(KeyFactory<>))
                continue;
            var keyfact = (KeyFactory)Activator.CreateInstance(fac);
            KeyFactory.factories.Add(fac.BaseType.GetGenericArguments()[0], keyfact);
        }
    }

    internal static object? CreateFor(Type keyType, object[] existingKeys)
    {
        if(factories.TryGetValue(keyType, out var factory))
        {
            return factory._NewKey(existingKeys);
        }
        return null;
    }
    protected internal abstract object _NewKey(object[] existingKeys);
}

public abstract class KeyFactory<T> : KeyFactory
{
    /// <summary>
    /// Do not override
    /// </summary>
    /// <param name="existingKeys"></param>
    /// <returns></returns>
    protected internal override object _NewKey(object[] existingKeys)
    {
        return NewKey(Reflection.TypeConverter.ConvertAll<T[]>(existingKeys));
    }

    public abstract Auto<T> NewKey(T[] existingKeys);
}

public class IntKeyFactory : KeyFactory<int>
{
    public override Auto<int> NewKey(int[] existingKeys)
    {
        return existingKeys.NextAvalible();
    }
}

public class GuidKeyFactory : KeyFactory<Guid>
{
    public override Auto<Guid> NewKey(Guid[] existingKeys) => Guid.CreateVersion7();
}