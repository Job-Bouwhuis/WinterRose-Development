using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.FrostWarden.Worlds
{
    public static class Universe
    {
        private static World currentWorld = null;
        private static World? nextWorld = null;

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
