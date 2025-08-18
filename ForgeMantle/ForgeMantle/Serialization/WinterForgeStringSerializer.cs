using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.WinterForgeSerializing;

namespace WinterRose.ForgeMantle.Serialization;
public class WinterForgeStringSerializer : IStringSerializer
{
    public object Deserialize(string raw) => WinterForge.DeserializeFromString(raw);
    public string Serialize(object snapshot) => WinterForge.SerializeToString(snapshot);
}

public class WinterForgeStringSerializer<T> : IStringSerializer<T>
{
    public T Deserialize(string raw) => WinterForge.DeserializeFromString<T>(raw);
    public string Serialize(T snapshot) => WinterForge.SerializeToString(snapshot);
}
