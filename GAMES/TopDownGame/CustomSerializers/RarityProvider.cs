using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame;
using WinterRose.WinterForgeSerializing;
using WinterRose.WinterForgeSerializing.Workers;

namespace TopDownGame.CustomSerializers;
internal class RarityProvider : CustomValueProvider<Rarity>
{
    public override Rarity? CreateObject(object value, WinterForgeVM executor)
    {
        if (value is "null")
            return null;
        return Rarity.GetRarity((int)value);
    }
    public override object CreateString(Rarity obj, ObjectSerializer serializer) 
    {
        return obj.Level;
    }
}
