using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using WinterRose.Reflection;
using vec3 = System.Numerics.Vector3;
using vec4 = System.Numerics.Vector4;

namespace WinterRose.Monogame.EditorMode.BuildInDisplays
{
    internal class EdtorColorDisplay : EditorDisplay<Color>
    {
        public override void Render(ref Color value, MemberData field, object obj)
        {
            if (field.HasAttribute<NoAlphaAttribute>())
            {
                vec3 vec3 = value.ToVector3().ToNumerics();
                gui.ColorEdit3(field.Name, ref vec3);
                Color newCol = new Color(vec3.X, vec3.Y, vec3.Z, 255);

                if (!field.HasAttribute<ReadonlyAttribute>() && field.CanWrite)
                    field.SetValue(ref obj, newCol);
            }
            else
            {
                vec4 vec4 = value.ToVector4().ToNumerics();
                gui.ColorEdit4(field.Name, ref vec4, ImGuiNET.ImGuiColorEditFlags.AlphaBar);
                Color newCol = new Color(vec4.X, vec4.Y, vec4.Z, vec4.W);

                if (!field.HasAttribute<ReadonlyAttribute>() && field.CanWrite)
                    field.SetValue(ref obj, newCol);
            }
        }
    }
}

namespace WinterRose.Monogame.EditorMode
{
    /// <summary>
    /// Instructs the editor to show a color editor without the alpha channel
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class NoAlphaAttribute : Attribute
    {

    }

    /// <summary>
    /// Instructs the editor to display this vector3 or vector4 instead of a vector.
    /// </summary>
    public class AsColorAttribute : Attribute
    {

    }
}
