using System.Text;
using System.Threading.Tasks;
using WinterRose.SourceGeneration.Serialization;

namespace WinterRose
{
    /// <summary>
    /// A range of floet values.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    [GenerateSerializer]
    public struct RangeF(IndexF start, IndexF end)
    {
        public IndexF Start { get; set; } = start;
        public IndexF End { get; set; } = end;

        public RangeF() : this(0, 0) { }

        public static implicit operator RangeF((float start, float end) range) => new(range.start, range.end);
    }
}
