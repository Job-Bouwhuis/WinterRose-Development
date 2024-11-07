using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SnowLibraryTesting
{
    public class Player
    {
        public int health;
        public int mana;
        public int level;

        private int xpTillLevelUp;

        public void GainXP(int xp)
        {
            xpTillLevelUp -= xp;
            if (xpTillLevelUp <= 0)
            {
                int overflow = -xpTillLevelUp;
                LevelUp();
                GainXP(overflow);
            }
        }

        private void LevelUp()
        {
            level++;
            xpTillLevelUp = (int)(level * 1.1f + 100_000);
        }


        public void GetFieldExample()
        {
            Type objectType = typeof(Player);

            var field = objectType.GetField(
                nameof(xpTillLevelUp), 
                BindingFlags.Instance | BindingFlags.NonPublic);

            Console.WriteLine(field!.GetValue(this));
        }

#pragma warning disable IDE0059 
#pragma warning disable CA1822

        public void BitwiseORExample()
        {
            int flags = 0b0000_0000;
            flags |= 0b0000_0001;
            // 0b0000_0001

            flags |= 0b0010_0000;
            // 0b0010_0001
        }

        public void BitwiseORExample2()
        {
            BindingFlags flags = BindingFlags.Instance;

            flags |= BindingFlags.NonPublic;
            flags |= BindingFlags.Public;

            // flags now holds the binding flags
            // Instance, NonPublic, and Public
        }

    }
}
