using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.FrostWarden.Components;

namespace WinterRose.FrostWarden
{
    public class RequireComponentAttribute<T> : Attribute where T : IComponent
    {
        public bool AutoAdd { get; set; } = false;
    }
}
