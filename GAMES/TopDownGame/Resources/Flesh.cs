using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame;

namespace TopDownGame.Resources;
internal class Flesh : Resource
{
    public override string Name => nameof(Flesh);
    public override string Description => "Flesh ripped off by a weapon from an enemy";
    public override Rarity Rarity => Constants.CommonRarity;
    
}
