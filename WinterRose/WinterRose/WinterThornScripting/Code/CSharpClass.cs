using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.WinterThornScripting
{
    public interface CSharpClass
    {
        void Constructor(Variable[] args);
        Class GetClass();
    }
}
