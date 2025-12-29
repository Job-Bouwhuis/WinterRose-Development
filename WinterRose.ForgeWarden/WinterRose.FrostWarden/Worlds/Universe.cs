using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.Editor;
using WinterRose.ForgeWarden.Input;

namespace WinterRose.ForgeWarden.Worlds
{
    public static class Universe
    {
        private static World currentWorld = null;
        private static World? nextWorld = null;

        public static Hierarchy Hirarchy { get; } = new();

        public static InputContext Input { get; }

        public static Vector3 SunDirection { get; set; } = new Vector3(0, -1, 0);

        static Universe()
        {
            Input = new InputContext(new RaylibInputProvider(), -int.MaxValue / 2, false);
            if (!ForgeWardenEngine.Current.Window.ConfigFlags.HasFlag(Raylib_cs.ConfigFlags.TransparentWindow))
                InputManager.RegisterContext(Input);
        }

        public static World CurrentWorld
        {
            get => currentWorld;
            set
            {
                if (currentWorld == null)
                {
                    currentWorld = value;
                    currentWorld.InitializeWorld();
                    return;
                }
                nextWorld = value;
            }
        }


        internal static void CommitWorldChangeIfNeeded()
        {
            if (nextWorld is not null)
            {
                // dispose calls the dispose on all related entities, and all entities call dispose on their components.
                // effectively disposing all unmanaged resources assuming each component didnt forget anything
                currentWorld.Dispose();

                currentWorld = nextWorld;
                nextWorld = null;

                currentWorld.InitializeWorld();
            }
        }

    }
}
