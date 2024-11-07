using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame.Worlds;
using WinterRose.WinterThornScripting;

namespace WinterRose.Monogame.WinterThornPort
{
    internal class ThornDebug : CSharpClass
    {
        public void Constructor(Variable[] args)
        {
        }

        public Class GetClass()
        {
            Class result = new("Debug", "A debug class");
            result.CSharpClass = this;

            result.DeclareFunction(new Function("Log", "Logs the specified object", AccessControl.Public)
            {
                CSharpFunction = (object o) =>
                {
                    Debug.Log(o);
                }
            });
            result.DeclareFunction(new Function("LogError", "Logs the specified object as an error", AccessControl.Public)
            {
                CSharpFunction = (object o) =>
                {
                    Debug.LogError(o);
                }
            });
            result.DeclareFunction(new Function("LogWarning", "Logs the specified object as a warning", AccessControl.Public)
            {
                CSharpFunction = (object o) =>
                {
                    Debug.LogWarning(o);
                }
            });
            result.DeclareFunction(new Function("ShowEditor", "Shows the in-app editor for worlds", AccessControl.Public)
            {
                CSharpFunction = () =>
                {
                    WorldEditor.Show = true;
                }
            });
            result.DeclareFunction(new Function("HideEditor", "Hides the in-app editor for worlds", AccessControl.Public)
            {
                CSharpFunction = () =>
                {
                    WorldEditor.Show = false;
                }
            });
            result.DeclareFunction(new Function("ShowHirarchy", "Shows the in-app hirarchy of the world, object, and compontents", AccessControl.Public)
            {
                CSharpFunction = () =>
                {
                    Hirarchy.Show = true;
                }
            });
            result.DeclareFunction(new Function("HideHirarchy", "Hides the in-app hirarchy of the world, object, and compontents", AccessControl.Public)
            {
                CSharpFunction = () =>
                {
                    Hirarchy.Show = false;
                }
            });

            return result;
        }
    }
}
