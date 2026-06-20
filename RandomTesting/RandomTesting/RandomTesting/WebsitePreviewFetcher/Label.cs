namespace RandomTesting.WebsitePreviewFetcher
{
    // ========== LABEL ==========
    public class Label : Control
    {
        public Label(string text)
        {
            Text = text;
            Width = text.Length;
            Height = 1;
            Focusable = false;
        }

        public override void Layout(int maxWidth, int maxHeight)
        {
            // fixed size
        }

        public override void Draw(int offsetX, int offsetY)
        {
            int drawX = offsetX + X;
            int drawY = offsetY + Y;
            Renderer.DrawString(drawX, drawY, new string(' ', Width), null, BackColor);
            Renderer.DrawString(drawX, drawY, Text, ForeColor, BackColor);
        }
    }
}