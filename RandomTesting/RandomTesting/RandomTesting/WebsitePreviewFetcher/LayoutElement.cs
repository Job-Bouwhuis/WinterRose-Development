namespace RandomTesting.WebsitePreviewFetcher
{
    public abstract class LayoutElement
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsVisible { get; set; } = true;
        public object Tag { get; set; }
        public LayoutElement Parent { get; set; }

        public abstract void Layout(int maxWidth, int maxHeight);
        public abstract void Draw(int offsetX, int offsetY);
        public virtual IEnumerable<Control> GetFocusableControls() => Enumerable.Empty<Control>();
        public virtual void OnWindowSet(Window window) { }
        public virtual void OnWindowUnset() { }
    }
}