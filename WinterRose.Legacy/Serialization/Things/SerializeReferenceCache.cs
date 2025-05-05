using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WinterRose.Legacy.Serialization.Things;
public class SerializeReferenceCache
{
    Lock Lock = new();
    public ConcurrentDictionary<int, object> Cache { get; } = [];

    int nextKey = 0;

    /// <summary>
    /// Maps the object in the cache, and returns the key of this object. if it already exists, returns the key of the existing object.<br></br><br></br>
    /// 
    /// Used in serialization.
    /// </summary>
    /// <param name="o"></param>
    /// <param name="key"></param>
    /// <returns>true if an object was added, false if an existing key was returned</returns>
    public bool Map(object o, out int key)
    {
        var obj = Cache.FirstOrDefault(x => x.Value.Equals(o));
        if (obj.Value == null)
        {
            key = Assign(o);
            return true;
        }
        key = obj.Key;
        return false;
    }

    /// <summary>
    /// Puts an object to the cache. Used when deserializing
    /// </summary>
    /// <param name="key"></param>
    /// <param name="o"></param>
    public void Map(int key, ref object o)
    {
        try
        {
            _ = Cache[key];
            throw new SerializeCacheException($"Object of key {key} already exists.");
        }
        catch
        {
            // object does not yet exist.
            Cache[key] = o;
        }
    }

    private int Assign(object o)
    {
        Lock.Enter();
        int k = nextKey;
        nextKey++;
        Lock.Exit();

        Cache[k] = o;
        return k;
    }

    /// <summary>
    /// Gets an object from the cache. Used for deserialization
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="SerializeCacheException"></exception>
    public object Get(int key)
    {
        try
        {
            return Cache[key];
        }
        catch
        {
            throw new SerializeCacheException($"Object of key {key} Does not exist.");
        }
    }

    private class SerializeCacheException(string msg) : Exception(msg);
}
