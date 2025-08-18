using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeMantle.Models;

namespace WinterRose.ForgeMantle.Serialization;
public interface IConfigStringSerializer : IStringSerializer<ConfigSnapshot>
{
    string Serialize(ConfigSnapshot snapshot);
    ConfigSnapshot Deserialize(string raw);

    void ISerializer<ConfigSnapshot>.Serialize(ConfigSnapshot snapshot, Stream dest)
    {
        string s = Serialize(snapshot);
        dest.Write(Encoding.UTF8.GetBytes(s));
        dest.Flush();
    }

    ConfigSnapshot ISerializer<ConfigSnapshot>.Deserialize(Stream raw)
    {
        using StreamReader reader = new StreamReader(raw);
        string s = reader.ReadToEnd();
        return Deserialize(s);
    }
}

public interface IStringSerializer : ISerializer
{
    string Serialize(object snapshot);
    object Deserialize(string raw);

    void ISerializer.Serialize(object snapshot, Stream dest)
    {
        string s = Serialize(snapshot);
        dest.Write(Encoding.UTF8.GetBytes(s));
        dest.Flush();
    }

    object ISerializer.Deserialize(Stream raw)
    {
        using StreamReader reader = new StreamReader(raw);
        string s = reader.ReadToEnd();
        return Deserialize(s);
    }
}

public interface IStringSerializer<T> : ISerializer<T>
{
    string Serialize(T snapshot);
    T Deserialize(string raw);

    void ISerializer<T>.Serialize(T snapshot, Stream dest)
    {
        string s = Serialize(snapshot);
        dest.Write(Encoding.UTF8.GetBytes(s));
        dest.Flush();
    }

    T ISerializer<T>.Deserialize(Stream raw)
    {
        using StreamReader reader = new StreamReader(raw);
        string s = reader.ReadToEnd();
        return Deserialize(s);
    }
}

public interface IConfigSerializer : ISerializer<ConfigSnapshot>;

public interface ISerializer
{
    void Serialize(object o, Stream dest);
    object? Deserialize(Stream raw);
}

public interface ISerializer<T> : ISerializer
{
    void Serialize(T o, Stream dest);
    T? Deserialize(Stream raw);

    object? ISerializer.Deserialize(Stream raw) => Deserialize(raw);
    void ISerializer.Serialize(object o, Stream dest) => Serialize((T)o, dest);
}
