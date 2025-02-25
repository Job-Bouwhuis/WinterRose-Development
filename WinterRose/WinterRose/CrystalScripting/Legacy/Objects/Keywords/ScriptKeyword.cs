using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.CrystalScripting.Legacy;
using WinterRose.CrystalScripting.Legacy.Objects.Base;

namespace WinterRose.CrystalScripting.Legacy.Objects.Keywords
{
    public abstract class ScriptKeyword
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string Usage { get; }
        public abstract string[] Aliases { get; }

        public abstract CrystalError Run(CrystalScript script);
    }
}


