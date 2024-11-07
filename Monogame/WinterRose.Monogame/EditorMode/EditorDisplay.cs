using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Reflection;

namespace WinterRose.Monogame
{
    /// <summary>
    /// Dictates how the hirarchy should display the object. Inherit from this class to create a custom display for this class or struct in the hirarchy.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class EditorDisplay<T>() : EditorDisplay(typeof(T), "super secret code")
    {
        /// <summary>
        /// Using ImGui, render the object in the hirarchy tree.
        /// </summary>
        /// <param name="obj"></param>
        public abstract void Render(ref T value, MemberData field, object obj);

        internal override void I_Render(ref object value, MemberData field, object obj)
        {
            T val = (T)value;

            Render(ref val, field, obj);
        }
    }

    public abstract class EditorDisplay
    {
        internal EditorDisplay(Type t, string superSecretCode)
        {
            if (superSecretCode != "super secret code")
                throw new Exception("You cannot inherit from this class");
            DisplayType = t;
        }

        public Type DisplayType { get; }

        internal abstract void I_Render(ref object obj, MemberData field, object owner);
    }
}
