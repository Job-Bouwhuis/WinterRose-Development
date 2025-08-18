using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.Components;

namespace WinterRose.ForgeWarden
{
    /// <summary>
    /// This components requires component <typeparamref name="T"/> to be present too in order to function properly
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RequireComponentAttribute<T> : Attribute where T : IComponent
    {
        /// <summary>
        /// Optionally instruct the system to automatically add a <typeparamref name="T"/> if it wasnt already present before
        /// </summary>
        public bool AutoAdd { get; set; } = false;
    }
}
