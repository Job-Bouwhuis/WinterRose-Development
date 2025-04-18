using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.UI
{
    /// <summary>
    /// This component does little on its own, however its the root of all screen space UI.
    /// <br></br> any child renderers of the object having this component will be rendered in screenspace, 
    /// rather than world space and always on top of the game world
    /// </summary>
    public class UICanvas : ObjectComponent
    {
    }
}
