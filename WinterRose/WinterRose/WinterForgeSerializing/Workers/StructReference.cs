using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.WinterForgeSerializing.Workers
{
    internal unsafe class StructReference(void* p)
    {
        public ref object Get() => ref *(object*)p;
    }
}
