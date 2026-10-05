namespace WinterRose.Formatting.TimeFormats
{
    public sealed record DurationCondition(
        DateComparison Comparison,
        TimeSpan Duration
    ) : DateCondition;
}