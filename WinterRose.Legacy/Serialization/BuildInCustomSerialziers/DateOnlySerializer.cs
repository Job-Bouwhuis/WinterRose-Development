using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Legacy.Serialization.Things;

namespace WinterRose.Legacy.Serialization.BuildInCustomSerialziers
{
    internal class DateOnlySerializer : CustomSerializer<DateOnly>
    {
        public override string Serialize(object obj, int depth)
        {
            DateOnly time = (DateOnly)obj;
            return $"-{depth}{time.Year}-{depth}{time.Month}-{depth}{time.Day}";
        }

        public override object Deserialize(string data, int depth)
        {
            DateOnly time;
            string[] values = data.Split($"-{depth}", StringSplitOptions.RemoveEmptyEntries);
            time = new DateOnly(
                TypeWorker.CastPrimitive<int>(values[0]),
                TypeWorker.CastPrimitive<int>(values[1]),
                TypeWorker.CastPrimitive<int>(values[2]));

            return time;
        }
    }
}
