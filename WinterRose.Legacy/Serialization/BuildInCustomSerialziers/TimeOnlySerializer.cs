using System;
using WinterRose.Legacy.Serialization.Things;

namespace WinterRose.Legacy.Serialization.BuildInCustomSerialziers
{
    internal class TimeOnlySerializer : CustomSerializer<TimeOnly>
    {
        public override string Serialize(object obj, int depth)
        {
            TimeOnly time = (TimeOnly)obj;
            return $"-{depth}{time.Hour}-{depth}{time.Minute}-{depth}{time.Second}-{depth}{time.Millisecond}";
        }

        public override object Deserialize(string data, int depth)
        {
            TimeOnly time;
            string[] values = data.Split($"-{depth}", StringSplitOptions.RemoveEmptyEntries);
            time = new TimeOnly(
                TypeWorker.CastPrimitive<int>(values[0]),
                TypeWorker.CastPrimitive<int>(values[1]),
                TypeWorker.CastPrimitive<int>(values[2]),
                TypeWorker.CastPrimitive<int>(values[3]));

            return time;
        }
    }
}
