namespace WinterRose.Formatting.TimeFormats
{
    public sealed record ConditionalNode(
        DateCondition Condition,
        DateFormatNode WhenTrue,
        DateFormatNode WhenFalse
    ) : DateFormatNode;
}