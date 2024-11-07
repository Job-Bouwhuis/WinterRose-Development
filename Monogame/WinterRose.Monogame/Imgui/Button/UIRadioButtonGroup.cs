using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.UserInterface.ImGuiInterface
{
    public sealed class UIRadioButtonGroup : ImGuiItem
    {
        public UIRadioButton ActiveButton { get; private set; }

        private UIRadioButton[] buttons;

        public UIRadioButtonGroup(params UIRadioButton[] buttons)
        {
            this.buttons = buttons;
            this.buttons.Foreach(x => x.OnClick += () => {
                x.owner = this;
                this.buttons.Foreach(y =>
                {
                    if (x == y)
                    {
                        ActiveButton = x;
                        return;
                    }
                    y.IsChecked = false;
                });
            });
        }

        public override void CreateItem()
        {
            foreach(var button in buttons)
            {
                button.CreateItem();
            }
        }
    }
}
