using Raylib_cs;
using WinterRose.ForgeWarden.Utility;

namespace WinterRose.ForgeWarden.UserInterface;
public class UIFreeform : UIContent
{
    // configuration
    public const float DEFAULT_FREEFORM_HEIGHT = 160f;
    public float Padding { get; set; } = UIConstants.CONTENT_PADDING;

    // storage: explicit position for each content (positions are offsets from the content origin:
    // top-left of the container's inner area, i.e. bounds.X + Padding, bounds.Y + Padding)
    public List<UIContent> Contents { get; } = new();
    private readonly Dictionary<UIContent, Vector2> contentPositions = new();

    public UIFreeform()
    {
    }

    protected internal override void Setup()
    {
        base.Setup();
        // ensure any children already present are setup
        foreach (var c in Contents)
        {
            c.Owner = Owner;
            c.Setup();
        }
    }

    /// <summary>
    /// Add content at a specific position (relative to the container's inner origin).
    /// </summary>
    public void AddContent(UIContent content, Vector2 position)
    {
        if (content == null) return;
        if (!Contents.Contains(content))
            Contents.Add(content);

        contentPositions[content] = position;
        content.Owner = Owner;
        content.Setup();
    }

    /// <summary>
    /// Remove content from the container.
    /// </summary>
    public void RemoveContent(UIContent content)
    {
        if (content == null) return;
        if (Contents.Remove(content))
        {
            content.OnOwnerClosing();
            contentPositions.Remove(content);
        }
    }

    /// <summary>
    /// Move an existing content to a new position (relative to inner origin).
    /// Returns false if the content is not present.
    /// </summary>
    public bool SetContentPosition(UIContent content, Vector2 position)
    {
        if (content == null) return false;
        if (!Contents.Contains(content)) return false;
        contentPositions[content] = position;
        return true;
    }

    /// <summary>
    /// Try get the position of a content (relative to inner origin).
    /// </summary>
    public bool TryGetContentPosition(UIContent content, out Vector2 position)
    {
        if (content == null)
        {
            position = Vector2.Zero;
            return false;
        }

        return contentPositions.TryGetValue(content, out position);
    }

    /// <summary>
    /// Enumerate all content and their positions (relative to inner origin).
    /// </summary>
    public IEnumerable<KeyValuePair<UIContent, Vector2>> GetAllContentPositions()
        => contentPositions.ToList().AsReadOnly();

    public override Vector2 GetSize(Rectangle availableArea)
    {
        float width = availableArea.Width;
        float innerWidth = Math.Max(0f, width - Padding * 2f);

        // Compute bounding extents of children based on their stored positions and measured sizes
        float maxRight = 0f;
        float maxBottom = 0f;
        foreach (var content in Contents)
        {
            if (!contentPositions.TryGetValue(content, out Vector2 pos))
                pos = Vector2.Zero;

            // available width for this child is reduced by its X offset
            float availableWidthForChild = Math.Max(0f, innerWidth - pos.X);

            // measure child size given the available width
            var measured = content.GetSize(new Rectangle(0, 0, (int)availableWidthForChild, int.MaxValue));

            float right = pos.X + measured.X + Padding * 2f;
            float bottom = pos.Y + measured.Y + Padding * 2f;

            maxRight = Math.Max(maxRight, right);
            maxBottom = Math.Max(maxBottom, bottom);
        }

        // The final width is the available width from caller; height is bounded by computed bottom or default
        float finalHeight = Math.Max(DEFAULT_FREEFORM_HEIGHT, maxBottom);
        return new Vector2(width, finalHeight);
    }

    protected internal override float GetHeight(float maxWidth)
    {
        return GetSize(new Rectangle(0, 0, (int)maxWidth, int.MaxValue)).Y;
    }

    protected internal override void Update()
    {
        foreach (var content in Contents)
            content.Update();
    }

    protected internal override void OnOwnerClosing()
    {
        foreach (var content in Contents)
            content.OnOwnerClosing();
    }

    protected internal override void OnHoverEnd()
    {
        foreach (var content in Contents)
            content.OnHoverEnd();
    }

    protected internal override void OnClickedOutsideOfContent(MouseButton button)
    {
        foreach (var content in Contents)
            content.OnClickedOutsideOfContent(button);
    }

    protected override void Draw(Rectangle bounds)
    {
        // clip everything to the container bounds
        ScissorStack.Push(bounds);

        float innerX = bounds.X + Padding;
        float innerY = bounds.Y + Padding;
        float innerWidth = Math.Max(0f, bounds.Width - Padding * 2f);

        foreach (var content in Contents)
        {
            if (!contentPositions.TryGetValue(content, out Vector2 pos))
                pos = Vector2.Zero;

            // available width is reduced by how far right the content starts
            float availableWidthForChild = Math.Max(0f, innerWidth - pos.X);

            // ask the child how big it wants to be given the constrained width
            Vector2 measured = content.GetSize(new Rectangle(0, 0, (int)availableWidthForChild, int.MaxValue));

            // final child bounds: position + measured size but clamped to available width
            float childX = innerX + pos.X;
            float childY = innerY + pos.Y;
            int childWidth = Math.Max(0, (int)Math.Min(availableWidthForChild, measured.X));
            int childHeight = Math.Max(0, (int)measured.Y);

            Rectangle childBounds = new Rectangle((int)childX, (int)childY, childWidth, childHeight);

            if (content.IsContentHovered(childBounds))
            {
                content.OnHover();
                content.IsHovered = true;

                foreach (var button in Enum.GetValues<MouseButton>())
                {
                    if (Input.IsPressed(button))
                        content.OnContentClicked(button);
                }
            }
            else
            {
                foreach (var button in Enum.GetValues<MouseButton>())
                    if (Input.IsPressed(button))
                        content.OnClickedOutsideOfContent(button);

                if (content.IsHovered)
                    content.OnHoverEnd();
                content.IsHovered = false;
            }

            // draw the child
            content.InternalDraw(childBounds);
        }

        ScissorStack.Pop();
    }
}

