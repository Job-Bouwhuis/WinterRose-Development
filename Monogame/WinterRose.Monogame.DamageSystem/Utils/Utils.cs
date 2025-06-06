using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame;
using WinterRose.Reflection;

namespace WinterRose.Monogame.DamageSystem
{
    internal static class Utils
    {
        /// <summary>
        /// Adds the specified component instance to a given<br></br><br></br>
        /// 
        /// "Force" because it accesses the component list through reflections. Use sparingly
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="component"></param>
        public static void ForceAttachComponent(WorldObject obj, ObjectComponent component)
        {
            object o = obj;
            ReflectionHelper objectRef = new(ref o);
            List<ObjectComponent> components = (List<ObjectComponent>)objectRef.GetValueFrom("components");
            components.Add(component);

            object oo = component;
            ReflectionHelper compRef = new(ref oo);
            compRef.SetValue("_owner", obj);
        }
    }
}
