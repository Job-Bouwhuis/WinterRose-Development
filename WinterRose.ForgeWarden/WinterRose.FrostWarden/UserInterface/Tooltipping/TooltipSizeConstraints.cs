namespace WinterRose.ForgeWarden.UserInterface.Tooltipping
{
    public class TooltipSizeConstraints
    {
        public Vector2 MinSize { get; set; } = new Vector2(120f, 24f);
        public Vector2 MaxSize { get; set; } = new Vector2(420f, 600f);

        /// <summary>
        /// When sampling widths between min and max, step by this amount.
        /// </summary>
        public float WidthSampleStep { get; set; } = 32f;

        public TooltipSizeConstraints()
        {

        }

        public TooltipSizeConstraints(Vector2 minSize, Vector2 maxSize, float widthSampleStep = 32f)
        {
            MinSize = minSize;
            MaxSize = maxSize;
            WidthSampleStep = widthSampleStep;
        }

        public TooltipSizeConstraints(Func<UIContainer, Vector2> customMeasureOverride, float widthSampleStep = 32f)
        {
            CustomMeasureOverride = customMeasureOverride;
            WidthSampleStep = widthSampleStep;
        }

        /// <summary>
        /// Optional override which, if present, returns the desired size for the tooltip based on its contents.
        /// </summary>
        public readonly Func<UIContainer, Vector2>? CustomMeasureOverride;
    }
}