using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Plugins
{
    /// <summary>
    /// Interface for plugins <br></br>
    /// This interface is but a possible 
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// Called when the plugin is loaded
        /// </summary>
        public void OnLoad();
        /// <summary>
        /// Called when the plugin is unloading
        /// </summary>
        public void OnUnload();

        /// <summary>
        /// Called when the plugin is running<br></br>
        /// Depending on the application, this may be called multiple times.
        /// </summary>
        public void Run();
    }
}
