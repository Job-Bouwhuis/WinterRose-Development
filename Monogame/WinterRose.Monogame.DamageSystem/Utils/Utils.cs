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
            ReflectionHelper<WorldObject> objectRef = new(ref obj);
            List<ObjectComponent> components = (List<ObjectComponent>)objectRef.GetValueFrom("components");
            components.Add(component);

            ReflectionHelper<ObjectComponent> compRef = new(ref component);
            compRef.SetValue("_owner", obj);
        }
    }
}
