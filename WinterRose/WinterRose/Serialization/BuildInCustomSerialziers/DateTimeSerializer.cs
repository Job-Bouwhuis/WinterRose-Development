using System;

namespace WinterRose.Serialization.BuildInCustomSerialziers
{
    internal class DateTimeSerializer : CustomSerializer<DateTime>
    {
        public override string Serialize(object obj, int depth)
        {
            DateTime time = (DateTime)obj;
            return $"-{depth}{time.Year}-{depth}{time.Month}-{depth}{time.Day}-{depth}{time.Hour}-{depth}{time.Minute}-{depth}{time.Second}-{depth}{time.Millisecond}";
        }

        public override object Deserialize(string data, int depth)
        {
            DateTime time;
            string[] values = data.Split($"-{depth}", StringSplitOptions.RemoveEmptyEntries);
            time = new DateTime(
                TypeWorker.CastPrimitive<int>(values[0]),
                TypeWorker.CastPrimitive<int>(values[1]),
                TypeWorker.CastPrimitive<int>(values[2]),
                TypeWorker.CastPrimitive<int>(values[3]),
                TypeWorker.CastPrimitive<int>(values[4]),
                TypeWorker.CastPrimitive<int>(values[5]));

            return time;
        }
    }
}
