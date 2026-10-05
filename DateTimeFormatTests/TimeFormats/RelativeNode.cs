namespace WinterRose.Formatting.TimeFormats
{
    public sealed record RelativeNode(
        bool Short,
        bool Calendar
    ) : DateFormatNode;
}