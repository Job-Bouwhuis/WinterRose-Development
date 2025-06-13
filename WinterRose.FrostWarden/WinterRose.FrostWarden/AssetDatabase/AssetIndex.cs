using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.AnonymousTypes;

namespace WinterRose.FrostWarden.AssetDatabase
{
    public class AssetIndex
    {
        public string Name { get; internal set; }
        public string Path { get; internal set; }
        public string[] Tag { get; internal set; }
        public Anonymous Metadata { get; internal set; }
    }
}
