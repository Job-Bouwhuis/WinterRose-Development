using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Vectors;

namespace SnowLibraryTesting
{
    internal class reftests
    {

        public void tests()
        {

        }

        public void Method1(Vector3 vec)
        {
            vec.x++;
            vec.y++;
        }

        public void Method2(ref Vector3 vec)
        {
            vec.x++;
            vec.y++;
        }

        public void Method3(in Vector3 vec)
        {
            //vec.x++; // Error
            //vec.y++; // Error
        }

        public void Method4(out Vector3 vec)
        {
            vec = new Vector3(1, 1, 1);
        }

    }
}
