using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using WinterRose.Reflection;

namespace WinterRose.Monogame.EditorMode.BuildInDisplays
{
    internal class EditorEnumDisplay : EditorDisplay<Enum>
    {
        public override void Render(ref Enum value, MemberData field, object obj)
        {
            if (gui.Button($"{field.Name}: {value}"))
            {
                gui.OpenPopup($"{field.Name}:enumselect");
            }
            if (gui.BeginPopup($"{field.Name}:enumselect"))
            {
                bool isFlags = value.GetType().GetCustomAttribute<FlagsAttribute>() != null;

                List<Enum> allValues = new List<Enum>();
                foreach (var enumVal in Enum.GetValues(value.GetType()))
                    allValues.Add((Enum)enumVal);

                int selected = allValues.IndexOf(value);
                int prevSelected = selected;
                int max = allValues.Max(x => gui.CalcTextSize(x.ToString()).X).CeilingToInt();

                if (isFlags)
                {
                    gui.Text("Unchecking all will automatically recheck the first value.");
                    bool[] bools = new bool[allValues.Count];

                    for (int i = 0; i < allValues.Count; i++)
                        if (value.HasFlag(allValues[i]))
                            bools[i] = true;

                    gui.BeginChild("checkboxset", new System.Numerics.Vector2(max + 40, 30 * allValues.Count));
                    for (int i = 0; i < allValues.Count; i++)
                    {
                        Enum? val = allValues[i];
                        bool b = bools[i];
                        gui.Checkbox(val.ToString(), ref b);
                        bools[i] = b;
                    }

                    List<Enum> values = [];
                    for (int i = 0; i < bools.Length; i++)
                    {
                        bool b = bools[i];
                        Enum val = allValues[i];

                        if (b)
                            values.Add(val);
                    }

                    if (values.Count == 0)
                    {
                        field.SetValue(ref obj, allValues[0]);
                        return;
                    }

                    Enum newValue = values[0];
                    for (int i = 1; i < values.Count(); i++)
                    {
                        Enum nextVal = values[i];
                        newValue = newValue.AddFlag(nextVal);
                    }


                    if (!field.HasAttribute<ReadonlyAttribute>() && field.CanWrite)
                        field.SetValue(ref obj, newValue);

                    gui.EndChild();
                }
                else
                {
                    gui.BeginChild("radioset", new System.Numerics.Vector2(max + 40, 30 * allValues.Count));
                    for (int i = 0; i < allValues.Count; i++)
                    {
                        Enum? val = allValues[i];

                        gui.RadioButton(val.ToString(), ref selected, i);
                    }
                    gui.EndChild();

                    if (selected != prevSelected)
                    {
                        if (!field.HasAttribute<ReadonlyAttribute>() && field.CanWrite)
                            field.SetValue(ref obj, allValues[selected]);
                    }
                }

               
                gui.EndPopup();
            }
        }
    }
}
