using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame;

namespace TopDownGame.Resources
{
    class Crystal : Resource
    {
        public override string Name => nameof(Crystal);
        public override string Description => "Its shiny!";
        public override Rarity Rarity => Constants.CommonRarity;
    }
}
