using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ImGuiApps
{
    /// <summary>
    /// Contains options for the size of a window.
    /// </summary>
    public class WindowSizeOptions
    {
        public WindowSizeOptions() { }
        public WindowSizeOptions(Vector2 size, WindowSizeOptionParams @params = WindowSizeOptionParams.None)
        {
            Size = size;
            Params = @params;
        }
        public WindowSizeOptions(int width, int height) : this(new(width, height)) { }

        /// <summary>
        /// The size of the window.
        /// </summary>
        public Vector2 Size { get; set; } = new(400, 400);
        /// <summary>
        /// When to apply the window size option.
        /// </summary>
        public WindowSizeOptionParams Params { get; set; } = WindowSizeOptionParams.None;

        public static implicit operator Vector2(WindowSizeOptions options) => options.Size;
    }

    /// <summary>
    /// When to apply the window size option.
    /// </summary>
    public enum WindowSizeOptionParams
    {
        /// <summary>
        /// No options.
        /// </summary>
        None = ImGuiCond.None,
        /// <summary>
        /// Only apply the option on first use ever of the window.
        /// </summary>
        FirstUseEver = ImGuiCond.FirstUseEver,
        /// <summary>
        /// Only apply the option if the window is appearing/opening
        /// </summary>
        Appearing = ImGuiCond.Appearing,
        /// <summary>
        /// Always apply the option.
        /// </summary>
        Always = ImGuiCond.Always
    }
}
