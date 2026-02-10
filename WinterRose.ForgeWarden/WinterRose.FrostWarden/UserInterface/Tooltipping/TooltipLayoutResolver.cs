using Raylib_cs;

namespace WinterRose.ForgeWarden.UserInterface.Tooltipping
{
    public static class TooltipLayoutResolver
    {
        public static Vector2 ResolveBestSize(Tooltip tooltip, TooltipSizeConstraints constraints)
        {
            if (constraints.CustomMeasureOverride != null)
            {
                Vector2 custom = constraints.CustomMeasureOverride(tooltip);
                return ClampToConstraints(custom, constraints);
            }

            // sample widths between min and max to find a good width/height combo
            float minW = Math.Max(8f, constraints.MinSize.X);
            float maxW = Math.Max(minW, constraints.MaxSize.X);

            List<Vector2> candidates = new List<Vector2>();

            // always try min and max
            candidates.Add(MeasureForWidth(tooltip, minW));
            if (maxW != minW)
                candidates.Add(MeasureForWidth(tooltip, maxW));

            // sample a few intermediate widths
            float step = Math.Max(1f, constraints.WidthSampleStep);
            for (float w = minW + step; w < maxW; w += step)
            {
                candidates.Add(MeasureForWidth(tooltip, w));
            }

            // pick the candidate with minimal area that fits within max height (if specified)
            Vector2 best = candidates[0];
            float bestScore = best.X * best.Y;

            for (int i = 1; i < candidates.Count; i++)
            {
                Vector2 c = candidates[i];
                Vector2 clamped = ClampToConstraints(c, constraints);
                float score = clamped.X * clamped.Y;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = clamped;
                }
            }

            return ClampToConstraints(best, constraints);
        }

        private static Vector2 MeasureForWidth(Tooltip tooltip, float width)
        {
            // temporarily set the container width so content measurement sees the right value
            Rectangle prev = tooltip.CurrentPosition;
            var tempRect = new Rectangle(prev.X, prev.Y, width, Math.Max(16f, tooltip.CurrentPosition.Height));
            tooltip.CurrentPosition = tempRect;

            float totalContentHeight = 0f;
            float availableWidth = width - UIConstants.CONTENT_PADDING * 2f;
            for (int i = 0; i < tooltip.Contents.Count; i++)
            {
                var content = tooltip.Contents[i];
                float ch = content.GetHeight(availableWidth);
                totalContentHeight += ch + UIConstants.CONTENT_PADDING;
            }

            var measured = new Vector2(
                width,
                Math.Max(16f, totalContentHeight + UIConstants.CONTENT_PADDING * 2f + (tooltip.Style.AllowUserResizing ? tooltip.Style.TitleBarHeight : 0f))
            );

            // restore previous
            tooltip.CurrentPosition = prev;

            return measured;
        }

        private static Vector2 ClampToConstraints(Vector2 size, TooltipSizeConstraints constraints)
        {
            float w = Math.Clamp(size.X, constraints.MinSize.X, constraints.MaxSize.X);
            float h = Math.Clamp(size.Y, constraints.MinSize.Y, constraints.MaxSize.Y);
            return new Vector2(w, h);
        }
    }
}