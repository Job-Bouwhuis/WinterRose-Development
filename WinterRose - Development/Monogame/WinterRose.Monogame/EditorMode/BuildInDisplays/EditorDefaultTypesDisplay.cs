using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Reflection;

namespace WinterRose.Monogame.EditorMode.BuildInDisplays
{
    internal class EditorBooleanDisplay : EditorDisplay<bool>
    {
        public override void Render(ref bool value, MemberData field, object obj)
        {
            gui.Checkbox($"{field.Name} (Boolean)", ref value);
            if (!field.HasAttribute<ReadonlyAttribute>() && field.CanWrite)
                field.SetValue(ref obj, value);
        }
    }

    internal class EditorFloatDisplay : EditorDisplay<float>
    {
        public override void Render(ref float value, MemberData field, object obj)
        {
            if (!field.HasAttribute<ReadonlyAttribute>() && field.CanWrite)
            {
                if (gui.Button("+"))
                    value++;
                gui.SameLine();
                if (gui.Button("-"))
                    value--;
                gui.SameLine();
            }

            if (field.Attributes.FirstOrDefault(x => x is SliderAttribute) is SliderAttribute range)
                gui.SliderFloat($"{field.Name} (Float)", ref value, range.Min, range.Max);
            else
                gui.InputFloat($"{field.Name} (Float)", ref value);


            if (!field.HasAttribute<ReadonlyAttribute>() && field.CanWrite)
                field.SetValue(ref obj, value);
        }
    }

    internal class EditorDoubleDisplay : EditorDisplay<double>
    {
        public override void Render(ref double value, MemberData field, object obj)
        {
            if (!field.HasAttribute<ReadonlyAttribute>() && field.CanWrite)
            {
                if (gui.Button("+"))
                    value++;
                gui.SameLine();
                if (gui.Button("-"))
                    value--;
                gui.SameLine();

            }
            gui.InputDouble($"{field.Name} (Double)", ref value);


            if (!field.HasAttribute<ReadonlyAttribute>() && field.CanWrite)
                field.SetValue(ref obj, value);
        }
    }

    internal class EditorIntDisplay : EditorDisplay<int>
    {
        public override void Render(ref int value, MemberData field, object obj)
        {
            if (!field.HasAttribute<ReadonlyAttribute>() && field.CanWrite)
            {
                if (gui.Button("+"))
                    value++;
                gui.SameLine();
                if (gui.Button("-"))
                    value--;
                gui.SameLine();
            }

            if (field.Attributes.FirstOrDefault(x => x is SliderAttribute) is SliderAttribute range)
                gui.SliderInt($"{field.Name} (Int)", ref value, range.Min.Round(), range.Max.Round());
            else
                gui.InputInt($"{field.Name} (Int)", ref value);


            if (!field.HasAttribute<ReadonlyAttribute>() && field.CanWrite)
                field.SetValue(ref obj, value);
        }
    }
}
