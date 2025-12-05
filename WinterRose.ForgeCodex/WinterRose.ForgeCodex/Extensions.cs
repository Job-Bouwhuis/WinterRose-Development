using System;
using System.Collections.Generic;
using System.Text;

namespace WinterRose.ForgeCodex;

public static class Extensions
{
    extension(int[] values)
    {
        /// <summary>
        /// finds the first unused <see cref="int"/> from a list
        /// </summary>
        /// <returns>the next avalible <see cref="int"/> from a list of type  <see cref="int"/></returns>
        public int NextAvalible()
        {
            ArgumentNullException.ThrowIfNull(values);

            var set = new HashSet<int>(values.Where(x => x > 0));
            int i = 1;
            while (true)
            {
                if (!set.Contains(i))
                    return i;
                i++;
            }
        }
    }
}
