using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ImGuiApps
{
    /// <summary>
    /// A default implementation of a menu bar that has a convenient way to add menu items and submenus.
    /// </summary>
    public class ImGuiMenuBar : ImGuiContent
    {
        public override void Render()
        {
            if (gui.BeginMainMenuBar())
            {
                foreach (var item in Content)
                {
                    if (item.Condition())
                        item.Render();
                }
                gui.EndMainMenuBar();
            }
        }

        public List<ImGuiMenuBarContent> Content { get; } = new();
    }

    public abstract class ImGuiMenuBarContent
    {
        /// <summary>
        /// A function to set the styles you want to use for the menu item.
        /// </summary>
        public Func<int> StyleFactory { get; set; } = () => { return 0; };

        /// <summary>
        /// A condition that must return true for the menu item to be rendered.
        /// </summary>
        public Func<bool> Condition { get; set; } = () => true;

        /// <summary>
        /// If false, the menu item will not be rendered.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        public abstract void Render();
    }

    /// <summary>
    /// A menu item that can have submenus (imagine a default 'File' menu in a desktop app, thats what this is for)
    /// </summary>
    /// <param name="name"></param>
    /// <param name="action"></param>
    public class ImGuiMenuItem(string name) : ImGuiMenuBarContent
    {
        public ImGuiMenuItem(string name, Action action) : this(name) => Action = action;
        public ImGuiMenuItem(string name, Action action, Func<bool> condition) : this(name, action) => Condition = condition;
        public string Name { get; set; } = name;
        public Action Action { get; set; } = delegate { };
        public List<ImGuiMenuItem> SubMenuItems { get; } = [];
        public bool Enabled { get; set; } = true;

        public ImGuiMenuItem AddSubMenus(params ImGuiMenuItem[] items)
        {
            SubMenuItems.AddRange(items);
            return this;
        }

        public override void Render()
        {
            if (SubMenuItems.Count > 0)
            {
                if (gui.BeginMenu(Name))
                {
                    foreach (var item in SubMenuItems)
                    {
                        if (item.Condition())
                            item.Render();
                    }
                    gui.EndMenu();
                }
            }
            else
            {
                if (gui.MenuItem(Name))
                {
                    Action?.Invoke();
                }
            }
        }
    }

    /// <summary>
    /// A label for the menu bar. (non-clickable, can be used to display information such as the app status or the current user)
    /// </summary>
    /// <param name="label"></param>
    public class ImGuiMenuLabel(string label) : ImGuiMenuBarContent
    {
        public string Label => label;

        public override void Render()
        {
            gui.Text(label);
        }
    }

    public class ImGuiClickableMenuLabel(string label, Action action) : ImGuiMenuBarContent
    {
        public string Label => label;
        public Action Action => action;

        public override void Render()
        {
            if (gui.MenuItem(label))
            {
                action?.Invoke();
            }
        }
    }

    /// <summary>
    /// A menu item that displays a loading bar.
    /// </summary>
    /// <param name="fractionFactory">The function that determains the fill level of the progress bar. and what should be displayed as text inside the progress bar. Should return a value between 0 and 1</param>
    /// <param name="width"></param>
    public class ImGuiMenuLoadingBar(Func<(string label, float fraction)> fractionFactory, float width = 250) : ImGuiMenuBarContent
    {
        public bool VisibleIf100Percent { get; set; } = true;

        public override void Render()
        {
            var (label, fraction) = fractionFactory();
            if (VisibleIf100Percent || fraction < 1)
                gui.ProgressBar(Math.Clamp(fraction, 0, 1), new Vector2(width, 0), label);
        }
    }

    /// <summary>
    /// A separator for in a menu context.
    /// </summary>
    public class ImGuiMenuSeparator() : ImGuiMenuItem("", null)
    {
        public override void Render()
        {
            gui.Separator();
        }
    }
}
