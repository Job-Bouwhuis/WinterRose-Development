using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeMantle.Models;
using WinterRose.WinterForgeSerializing;

namespace WinterRose.ForgeMantle.Serialization;
public class WinterForgeSerializer : IConfigSerializer
{
    public void Serialize(ConfigSnapshot snapshot, Stream dest) => WinterForge.SerializeToStream(snapshot, dest);
    public ConfigSnapshot Deserialize(Stream raw) => WinterForge.DeserializeFromStream<ConfigSnapshot>(raw);
}
