namespace RandomTesting.WebsitePreviewFetcher
{
    // ========== LAYER STACK ==========
    public class LayerStack : LayoutElement
    {
        public Orientation Orientation { get; set; }
        public List<LayoutElement> Children { get; } = new List<LayoutElement>();
        public int Spacing { get; set; } = 0;
        public bool ExpandChildren { get; set; } = false;

        public LayerStack(Orientation orientation)
        {
            Orientation = orientation;
        }

        public void AddChild(LayoutElement child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        public void AddControl(Control child) // convenience
        {
            child.Parent = this;
            Children.Add(child);
        }

        public override void Layout(int maxWidth, int maxHeight)
        {
            if (!IsVisible) { Width = 0; Height = 0; return; }

            if (Orientation == Orientation.Vertical)
            {
                int totalHeight = 0;
                int maxChildWidth = 0;
                foreach (var child in Children)
                {
                    if (!child.IsVisible) continue;
                    child.Layout(maxWidth, int.MaxValue);
                    maxChildWidth = Math.Max(maxChildWidth, child.Width);
                    totalHeight += child.Height + Spacing;
                }
                if (Children.Count > 0) totalHeight -= Spacing;

                Width = Math.Min(maxWidth, maxChildWidth);
                Height = totalHeight;

                int y = 0;
                foreach (var child in Children)
                {
                    if (!child.IsVisible) continue;
                    child.X = 0;
                    child.Y = y;
                    if (ExpandChildren)
                        child.Width = Width;
                    y += child.Height + Spacing;
                }
            }
            else // Horizontal
            {
                int rowWidth = 0;
                int rowHeight = 0;
                int totalHeight = 0;
                int maxRowWidth = 0;
                int x = 0;
                int y = 0;

                foreach (var child in Children)
                {
                    if (!child.IsVisible) continue;

                    child.Layout(maxWidth, maxHeight);
                    int spacing = rowWidth == 0 ? 0 : Spacing;
                    bool wraps = rowWidth > 0 && rowWidth + spacing + child.Width > maxWidth;

                    if (wraps)
                    {
                        maxRowWidth = Math.Max(maxRowWidth, rowWidth);
                        totalHeight += rowHeight + Spacing;
                        x = 0;
                        y = totalHeight;
                        rowWidth = 0;
                        rowHeight = 0;
                        spacing = 0;
                    }

                    child.X = x + spacing;
                    child.Y = y;
                    x += child.Width + Spacing;
                    rowWidth += spacing + child.Width;
                    rowHeight = Math.Max(rowHeight, child.Height);
                }

                if (rowWidth > 0)
                {
                    maxRowWidth = Math.Max(maxRowWidth, rowWidth);
                    totalHeight += rowHeight;
                }

                Width = Math.Min(maxWidth, maxRowWidth);
                Height = Math.Min(maxHeight, totalHeight);

                if (ExpandChildren)
                {
                    foreach (var child in Children)
                        if (child.IsVisible)
                            child.Height = Height;
                }
            }
        }

        public override void Draw(int offsetX, int offsetY)
        {
            if (!IsVisible) return;
            foreach (var child in Children)
            {
                if (!child.IsVisible) continue;
                child.Draw(offsetX + X, offsetY + Y);
            }
        }

        public override IEnumerable<Control> GetFocusableControls()
        {
            foreach (var child in Children)
                foreach (var c in child.GetFocusableControls())
                    yield return c;
        }

        public override void OnWindowSet(Window window)
        {
            foreach (var child in Children)
                child.OnWindowSet(window);
        }

        public override void OnWindowUnset()
        {
            foreach (var child in Children)
                child.OnWindowUnset();
        }
    }
}
