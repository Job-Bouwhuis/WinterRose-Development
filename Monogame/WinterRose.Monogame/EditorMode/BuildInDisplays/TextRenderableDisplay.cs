using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame.TextRendering;
using WinterRose.Reflection;

namespace WinterRose.Monogame.EditorMode.BuildInDisplays
{
    class TextRenderableDisplay : EditorDisplay<Text>
    {
        private Dictionary<int, string> table = []; 

        public override void Render(ref Text value, MemberData field, object obj)
        {
            var hash = value.GetHashCode();
            if (!table.TryGetValue(hash, out string? text))
            {
                table.Add(hash, value.ToString());
                text = "";
            }

            if (gui.InputTextWithHint($"TextInput{hash}", "New text", ref text, 1000, ImGuiNET.ImGuiInputTextFlags.EnterReturnsTrue))
            {
                value.SetText(text);
                table.Remove(hash);
            }
        }
    }
}
