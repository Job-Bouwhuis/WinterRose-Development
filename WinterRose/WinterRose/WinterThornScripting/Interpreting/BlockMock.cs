using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.WinterThornScripting.Interpreting
{
    internal class BlockMockk(Block mockedblock) : Block(mockedblock.Parent)
    {
        public override Function[] Functions => mockedblock.Functions;
        public override List<Variable> Variables => mockedblock.Variables;
    }
}
