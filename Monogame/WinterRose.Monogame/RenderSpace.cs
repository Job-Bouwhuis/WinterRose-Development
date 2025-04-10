using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame
{
    /// <summary>
    /// A state to indicate if a <see cref="WorldObject"/> is to use world positions or screen positions for its logic
    /// </summary>
    public enum RenderSpace
    {
        /// <summary>
        /// Use world space for logic.
        /// </summary>
        World,
        /// <summary>
        /// Use screen space for logic.
        /// </summary>
        Screen
    }
}
