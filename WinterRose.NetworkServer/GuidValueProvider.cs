using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.WinterForgeSerializing;
using WinterRose.WinterForgeSerializing.Workers;

namespace WinterRose.NetworkServer
{
    class GuidValueProvider : CustomValueProvider<Guid>
    {
        public override Guid CreateObject(string value, InstructionExecutor executor) => Guid.Parse(value);
        public override string CreateString(Guid obj, ObjectSerializer serializer) => obj.ToString();
    }
}
