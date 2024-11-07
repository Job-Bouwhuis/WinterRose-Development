using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Reflection;
using nVec4 = System.Numerics.Vector4;

namespace WinterRose.Monogame.EditorMode.BuildInDisplays
{
    internal class EditorVector4Display : EditorDisplay<Vector4>
    {
        public override void Render(ref Vector4 obj, MemberData field, object owner)
        {
            nVec4 vec = obj.ToNumerics();
            gui.InputFloat4(field.Name, ref vec);


            if (!field.HasAttribute<ReadonlyAttribute>() && field.CanWrite)
            {
                Vector4 v = vec;
                field.SetValue(ref owner, v);
            }
        }
    }

    internal class EditorNumericsVector4Display : EditorDisplay<nVec4>
    {
        public override void Render(ref nVec4 vec, MemberData field, object owner)
        {
            nVec4 v = vec;
            gui.InputFloat4(field.Name, ref v);

            if (!field.HasAttribute<ReadonlyAttribute>() && field.CanWrite)
            {
                field.SetValue(ref owner, v);
            }
        }
    }
}
