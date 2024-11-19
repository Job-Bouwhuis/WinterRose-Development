using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ImGuiApps
{
    /// <summary>
    /// Event arguments for when the application is about to close.
    /// </summary>
    public class ApplicationCloseEventArgs
    {
        /// <summary>
        /// When set to true, the application will not close.
        /// </summary>
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Event arguments for when a window is about to close.
    /// </summary>
    public class WindowCloseEventArgs
    {
        /// <summary>
        /// When set to true, the window will not close.
        /// </summary>
        public bool Cancel { get; set; }
    }
}
