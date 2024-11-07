using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Serialization.BuildInCustomSerialziers
{
    internal class DateTimeSerializer : ICustomSerializer
    {
        public Type SerializerType => typeof(DateTime);

        public string Serialize(object obj, int depth)
        {
            DateTime time = (DateTime)obj;
            return $"-{depth}{time.Year}-{depth}{time.Month}-{depth}{time.Day}-{depth}{time.Hour}-{depth}{time.Minute}-{depth}{time.Second}-{depth}{time.Millisecond}";
        }

        public object Deserialize(string data, int depth)
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

    internal class TimeSpanSerializer : ICustomSerializer
    {
        public Type SerializerType => typeof(TimeSpan);

        public string Serialize(object obj, int depth)
        {
            TimeSpan time = (TimeSpan)obj;
            return $"-{depth}{time.Days}-{depth}{time.Hours}-{depth}{time.Minutes}-{depth}{time.Seconds}-{depth}{time.Milliseconds}";
        }

        public object Deserialize(string data, int depth)
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

    internal class TimeOnlySerializer : ICustomSerializer
    {
        public Type SerializerType => typeof(TimeOnly);

        public string Serialize(object obj, int depth)
        {
            TimeOnly time = (TimeOnly)obj;
            return $"-{depth}{time.Hour}-{depth}{time.Minute}-{depth}{time.Second}-{depth}{time.Millisecond}";
        }

        public object Deserialize(string data, int depth)
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

    internal class DateOnlySerializer : ICustomSerializer
    {
        public Type SerializerType => typeof(DateOnly);

        public string Serialize(object obj, int depth)
        {
            DateOnly time = (DateOnly)obj;
            return $"-{depth}{time.Year}-{depth}{time.Month}-{depth}{time.Day}";
        }

        public object Deserialize(string data, int depth)
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
