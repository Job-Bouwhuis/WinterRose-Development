using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose;
using WinterRose.Vectors;
using WinterRose.WIP.TestClasses;

namespace SnowLibraryTesting
{
    internal class ListPropertySerializeTests
    {
        [IncludeWithSerialization]
        public string test1 { get; set; } = "THIS IS TEST 1";

        [IncludeWithSerialization]
        public string test2 { get; set; }

        [IncludeWithSerialization]
        public Vector3 Vec { get; set; } = new(1, 2, 3);

        public ListPropertySerializeTests()
        {
            test1 = Randomness.RandomString(Random.Shared.Next(5, 15));
            test2 = Randomness.RandomString(Random.Shared.Next(2, 20));

            int[] ints = Randomness.RandomInts(3);

            Vec = new Vector3(ints[0], ints[1], ints[2]);
        }
    }
}
