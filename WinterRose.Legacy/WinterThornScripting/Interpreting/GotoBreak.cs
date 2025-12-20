namespace WinterRose.WinterThornScripting.Interpreting
{
    internal class GotoBreak(int depthToBreak, int labelIndex) : Variable("GotoBreakout", "To breakout of blocks due to traveling to a label", AccessControl.Private)
    {
        public int CountOfBlocksToBreakOut { get; set; } = depthToBreak;
        public int LabelIndex { get; set; } = labelIndex;
    }
}