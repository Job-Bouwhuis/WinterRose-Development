using Microsoft.Xna.Framework;
using WinterRose.Reflection;
using nVec2 = System.Numerics.Vector2;

namespace WinterRose.Monogame.EditorDisplays
{
    internal class EditorVector2Display : EditorDisplay<Vector2>
    {
        public override void Render(ref Vector2 obj, MemberData field, object owner)
        {
            nVec2 vec = obj.ToNumerics();
            gui.InputFloat2(field.Name, ref vec);


            if (!field.HasAttribute<ReadonlyAttribute>() && field.CanWrite)
            {
                Vector2 v = vec;
                field.SetValue(ref owner, v);
            }
  
        }
    }

    internal class EditorNumericsVector2Display : EditorDisplay<nVec2>
    {
        public override void Render(ref nVec2 vec, MemberData field, object owner)
        {
            nVec2 v = vec;
            gui.InputFloat2(field.Name, ref v);

            if (!field.HasAttribute<ReadonlyAttribute>() && field.CanWrite)
            {
                field.SetValue(ref owner, v);
            }
        }
    }
}
