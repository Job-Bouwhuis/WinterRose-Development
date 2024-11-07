using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nVec3 = System.Numerics.Vector3;
using WinterRose.Reflection;
using Microsoft.Xna.Framework;

namespace WinterRose.Monogame.EditorMode.BuildInDisplays
{
    internal class EditorVector3Display : EditorDisplay<Vector3>
    {
        public override void Render(ref Vector3 obj, MemberData field, object owner)
        {
            nVec3 vec = obj.ToNumerics();
            gui.InputFloat3(field.Name, ref vec);


            if (!field.HasAttribute<ReadonlyAttribute>() && field.CanWrite)
            {
                Vector3 v = vec;
                field.SetValue(ref owner, v);
            }
        }
    }

    internal class EditorNumericsVector3Display : EditorDisplay<nVec3>
    {
        public override void Render(ref nVec3 vec, MemberData field, object owner)
        {
            nVec3 v = vec;
            gui.InputFloat3(field.Name, ref v);


            if (!field.HasAttribute<ReadonlyAttribute>() && field.CanWrite)
            {
                field.SetValue(ref owner, v);
            }
        }
    }
}
