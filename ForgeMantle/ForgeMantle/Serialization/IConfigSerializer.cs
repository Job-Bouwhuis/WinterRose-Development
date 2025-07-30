using ForgeMantle.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForgeMantle.Serialization;
public interface IConfigStringSerializer : IConfigSerializer
{
    string Serialize(ConfigSnapshot snapshot);
    ConfigSnapshot Deserialize(string raw);

    void IConfigSerializer.Serialize(ConfigSnapshot snapshot, Stream dest)
    {
        string s = Serialize(snapshot);
        dest.Write(Encoding.UTF8.GetBytes(s));
        dest.Flush();
    }

    ConfigSnapshot IConfigSerializer.Deserialize(Stream raw)
    {
        using StreamReader reader = new StreamReader(raw);
        string s = reader.ReadToEnd();
        return Deserialize(s);
    }
}

public interface IConfigSerializer
{
    void Serialize(ConfigSnapshot snapshot, Stream dest);
    ConfigSnapshot Deserialize(Stream raw);
}
