namespace RandomTesting.ConsoleWindowing
{
    public abstract class Control : LayoutElement, IConsumesNavigation
    {
        public Window Window { get; set; }
        public string Text { get; set; } = "";
        public ConsoleColor ForeColor { get; set; } = ConsoleColor.White;
        public ConsoleColor BackColor { get; set; } = ConsoleColor.Black;
        public bool Focusable { get; protected set; } = true;
        public bool Enabled { get; set; } = true;
        public int ZIndex { get; set; } = 0;

        public bool IsFocused { get; internal set; }
        public bool IsDirty { get; set; } = true;

        public virtual bool IsActive { get; protected set; }
        public virtual bool ConsumesNavigation => false;

        public (int X, int Y) GetAbsolutePosition()
        {
            int absX = this.X;
            int absY = this.Y;
            var parent = this.Parent;
            while (parent != null)
            {
                absX += parent.X;
                absY += parent.Y;
                parent = parent.Parent;
            }
            return (absX, absY);
        }

        public override IEnumerable<Control> GetFocusableControls()
        {
            if (Focusable && Enabled)
                yield return this;
        }

        public virtual void HandleKey(ConsoleKeyInfo key) { }
        public virtual void OnFocus() { }
        public virtual void OnBlur() { }
        public override void OnWindowSet(Window window) => Window = window;
        public override void OnWindowUnset() => Window = null;
        public virtual void Activate() { }
        public virtual void Cancel() { }

        public void MarkDirty()
        {
            IsDirty = true;
            if (Window != null) Window.MarkDirty();
        }
    }
}
