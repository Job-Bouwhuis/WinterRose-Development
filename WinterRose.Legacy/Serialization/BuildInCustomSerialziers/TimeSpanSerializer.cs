using System;
using WinterRose.Legacy.Serialization.Things;

namespace WinterRose.Legacy.Serialization.BuildInCustomSerialziers
{
    internal class TimeSpanSerializer : CustomSerializer<TimeSpan>
    {
        public override string Serialize(object obj, int depth)
        {
            TimeSpan time = (TimeSpan)obj;
            return $"-{depth}{time.Days}-{depth}{time.Hours}-{depth}{time.Minutes}-{depth}{time.Seconds}-{depth}{time.Milliseconds}";
        }

        public override object Deserialize(string data, int depth)
        {
            TimeSpan time;
            string[] values = data.Split($"-{depth}", StringSplitOptions.RemoveEmptyEntries);
            time = new TimeSpan(
                TypeWorker.CastPrimitive<int>(values[0]),
                TypeWorker.CastPrimitive<int>(values[1]),
                TypeWorker.CastPrimitive<int>(values[2]),
                TypeWorker.CastPrimitive<int>(values[3]),
                TypeWorker.CastPrimitive<int>(values[4]));

            return time;
        }
    }
}
