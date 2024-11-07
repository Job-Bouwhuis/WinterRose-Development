using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;
using System.Reflection;

namespace WinterRose.WIP.DynamicAssemblies
{
    public class DynamicAssemblyTests
    {
        public Assembly Create()
        {
            AssemblyName name = new("TestAssembly");

            AssemblyBuilder builder = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndCollect);

            return null;
        }
    }
}
