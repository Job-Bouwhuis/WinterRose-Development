using System.Text;
using System.Threading.Tasks;

namespace WinterRose
{
    /// <summary>
    /// A range of floet values.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    public struct RangeF(IndexF start, IndexF end)
    {
        [IncludeWithSerialization]
        public IndexF Start { get; set; } = start;
        [IncludeWithSerialization]
        public IndexF End { get; set; } = end;

        public RangeF() : this(0, 0) { }

        public static implicit operator RangeF((float start, float end) range) => new(range.start, range.end);
    }
}
