using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Legacy.Serialization.Things;

namespace WinterRose.Legacy.Serialization.BuildInCustomSerialziers
{
    internal class GuidSerializer : CustomSerializer<Guid>
    {
        public override string Serialize(object obj, int depth)
        {
            return obj.ToString();
        }

        public override object Deserialize(string data, int depth)
        {
            return Guid.Parse(data);
        }
    }
}
