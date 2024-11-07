using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.StatusSystem
{
    /// <summary>
    /// when and how often a status effect should update
    /// </summary>
    public enum StatusEffectUpdateType
    {
        /// <summary>
        /// The status effect is only updated on apply, and on removal
        /// </summary>
        Static,

        /// <summary>
        /// The status effect is updated per stack removal. eg from 2 to 1 stacks, and 1 to 0 stacks
        /// </summary>
        StackRemoval,

        /// <summary>
        /// The status effect is updated every frame
        /// </summary>
        Always
    }
}
