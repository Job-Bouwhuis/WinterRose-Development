using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeSignal;
using WinterRose.ForgeWarden.Worlds;

namespace WinterRose.ForgeWarden.UserInterface;
public class UITreeNode : UIContent
{
    private static readonly Dictionary<UITreeNode, (UITreeNode node, double time)> GlobalLastClickByRoot = new Dictionary<UITreeNode, (UITreeNode, double)>();
    private readonly Dictionary<UIContent, Rectangle> ChildLastRowRects = new Dictionary<UIContent, Rectangle>();

    public string Text { get; set; }
    public object Context { get; set; }

    public List<UIContent> Children { get; } = new List<UIContent>();
    public UITreeNode Parent { get; private set; }

    public bool IsCollapsed { get; private set; } = false;

    public MulticastVoidInvocation<UITreeNode> ClickInvocation { get; set; } = new();
    public MulticastVoidInvocation<UITreeNode> DoubleClickInvocation { get; set; } = new();

    // Internal click state (no leading underscores)
    private double lastClickTime = -1.0;
    private double pendingSingleClickUntil = -1.0;

    private Rectangle lastRowRect = new Rectangle();

    // Constructors / setup
    public UITreeNode(string text = "", object context = null)
    {
        Text = text;
        Context = context;
    }

    public UITreeNode(string text, Action<UITreeNode> configurator, object context = null)
    {
        Text = text;
        Context = context;
        configurator(this);
    }

    public void AddChild(UIContent child)
    {
        if (child == null) return;

        // if the child is a TreeNode, detach it from its previous parent
        if (child is UITreeNode tn && tn.Parent != null)
            tn.Parent.RemoveChild(tn);

        Children.Add(child);

        // If the child is a TreeNode, set its parent
        if (child is UITreeNode treeChild)
            treeChild.Parent = this;

        // adopt owner and run setup so child is ready immediately
        if (owner != null)
        {
            child.owner = owner;
            child.Setup();
        }
    }

    public bool RemoveChild(UIContent child)
    {
        if (child == null) return false;
        var removed = Children.Remove(child);
        if (removed)
        {
            // clear TreeNode parent link when applicable
            if (child is UITreeNode treeChild)
                treeChild.Parent = null;
            // remove any stored hit rect
            ChildLastRowRects.Remove(child);
        }
        return removed;
    }

    public void ToggleCollapsed() => IsCollapsed = !IsCollapsed;
    public void Collapse() => IsCollapsed = true;
    public void Expand() => IsCollapsed = false;

    /// <summary>
    /// Call this when the arrow/disclosure widget is clicked.
    /// (Some UI renderers may call this directly from the arrow click handler.)
    /// </summary>
    public void HandleArrowClick()
    {
        ToggleCollapsed();
    }

    // Layout & drawing -----------------------------------------------------

    public override Vector2 GetSize(Rectangle availableArea)
    {
        // Calculate own row
        float height = Style.TreeNodeHeight;

        // Prepare child layout area
        float childWidth = Math.Max(0f, availableArea.Width - Style.TreeNodeIndentWidth);
        float childX = availableArea.X + Style.TreeNodeIndentWidth;
        float childY = availableArea.Y + Style.TreeNodeHeight;

        // For each child calculate its height, give it the exact bounds we computed,
        // and let it perform its own GetSize with that specific child bounds.
        if (!IsCollapsed)
            foreach (var child in Children)
            {
                float childHeight = child.GetHeight(childWidth);
                var childBounds = new Rectangle((int)childX, (int)childY, (int)childWidth, (int)Math.Ceiling(childHeight));
                child.GetSize(childBounds); // delegate the bounded call to the child
                height += childHeight;
                childY += childHeight;
            }

        return new Vector2(availableArea.Width, height);
    }

    protected internal override float GetHeight(float maxWidth)
    {
        if (owner is null)
        {
            if (Parent != null)
            {
                owner = Parent.owner;
            }
        }

        float height = Style.TreeNodeHeight;

        if (!IsCollapsed)
        {
            // children are indented, so pass a reduced width to them
            float childMaxWidth = Math.Max(0f, maxWidth - Style.TreeNodeIndentWidth);
            foreach (var child in Children)
            {
                height += child.GetHeight(childMaxWidth);
            }
        }

        return height;
    }

    private UITreeNode GetRoot()
    {
        var cur = this;
        while (cur.Parent != null) cur = cur.Parent;
        return cur;
    }

    private bool IsAncestorOf(UITreeNode node)
    {
        if (node == null) return false;
        var cur = node;
        while (cur != null)
        {
            if (cur == this) return true;
            cur = cur.Parent;
        }
        return false;
    }

    protected override void Draw(Rectangle bounds)
    {
        // Row rectangle for this node
        var rowRect = new Rectangle(bounds.X, bounds.Y, bounds.Width, Style.TreeNodeHeight);

        // store the row rect so click handling can test whether the click actually hit this node
        lastRowRect = rowRect;

        // Mouse position
        float mx = Input.MousePosition.X;
        float my = Input.MousePosition.Y;
        bool mouseOverRow = mx >= rowRect.X && mx <= rowRect.X + rowRect.Width && my >= rowRect.Y && my <= rowRect.Y + rowRect.Height;
        
        // Background and hover highlight
        Raylib_cs.Raylib.DrawRectangleRec(rowRect, Style.TreeNodeBackground);
        if (mouseOverRow)
        {
            Raylib_cs.Raylib.DrawRectangleRec(rowRect, Style.TreeNodeHover);
        }

        // Arrow area (disclosure) and click handling
        var arrowRect = new Rectangle(bounds.X, bounds.Y, Style.TreeNodeIndentWidth, Style.TreeNodeHeight);
        if (Children.Count > 0)
        {
            // Simple arrow symbol (right = collapsed, down = expanded)
            string arrowSymbol = IsCollapsed ? ">" : "v";
            Raylib_cs.Raylib.DrawText(arrowSymbol, (int)(arrowRect.X + 4), (int)(arrowRect.Y + 2), 14, Style.TreeNodeArrow);

            // Detect arrow click (single click toggles collapse)
            if (mx >= arrowRect.X && mx <= arrowRect.X + arrowRect.Width && my >= arrowRect.Y && my <= arrowRect.Y + arrowRect.Height)
            {
                if (Input.IsPressed(MouseButton.Left))
                {
                    HandleArrowClick();
                }
            }
        }

        // Node text
        var textX = (int)(bounds.X + Style.TreeNodeIndentWidth + 4);
        var textY = (int)(bounds.Y + (Style.TreeNodeHeight - 14) / 2f);
        Raylib_cs.Raylib.DrawText(Text ?? string.Empty, textX, textY, 14, Style.TreeNodeText);

        // Outline for the row (subtle)
        Raylib_cs.Raylib.DrawRectangleLinesEx(rowRect, 1, Style.TreeNodeBorder);

        // Draw children below, indented
        float y = bounds.Y + Style.TreeNodeHeight;
        if (!IsCollapsed)
        {
            float childWidth = Math.Max(0f, bounds.Width - Style.TreeNodeIndentWidth);
            foreach (var child in Children)
            {
                
                float childHeight = child.GetHeight(childWidth);
                var childBounds = new Rectangle(bounds.X + Style.TreeNodeIndentWidth, (int)y, (int)childWidth, (int)childHeight);

                child.InternalDraw(childBounds);

                ChildLastRowRects[child] = childBounds;

                y += childHeight;
            }
        }
    }

    // Interaction & updates -----------------------------------------------

    protected internal override void Setup()
    {
        // Delegate setup to children
        foreach (var child in Children)
        {
            child.Setup();
        }
    }

    protected internal override void Update()
    {
        if (owner is null)
        {
            if (Parent != null)
            {
                owner = Parent.owner;
            }
        }

        // If a single-click is pending and its timeout passed, fire it now.
        var now = DateTime.UtcNow;
        var nowSec = (now - DateTime.UnixEpoch).TotalSeconds;

        if (pendingSingleClickUntil > 0 && nowSec >= pendingSingleClickUntil)
        {
            pendingSingleClickUntil = -1;
            lastClickTime = -1;
            ClickInvocation?.Invoke(this);
        }

        // Update children (so they can manage their own internal state)
        foreach (var child in Children)
        {
            child.Update();
        }
    }

    protected internal override void OnContentClicked(MouseButton button)
    {
        // Ensure the click physically happened inside this node's row rect.
        float mx = Input.MousePosition.X;
        float my = Input.MousePosition.Y;
        bool insideThisRow = mx >= lastRowRect.X && mx <= lastRowRect.X + lastRowRect.Width
                             && my >= lastRowRect.Y && my <= lastRowRect.Y + lastRowRect.Height;

        if (!insideThisRow)
        {
            // If the click wasn't on this node's row, see if it hits any child.
            // Delegate to the first child whose stored rect contains the click.
            foreach (var child in Children)
            {
                if (!ChildLastRowRects.TryGetValue(child, out var rect)) continue;
                if (rect.Width <= 0 || rect.Height <= 0) continue;

                bool insideChildRow = mx >= rect.X && mx <= rect.X + rect.Width
                                      && my >= rect.Y && my <= rect.Y + rect.Height;

                if (insideChildRow)
                {
                    // delegate the click to that child (child will handle further delegation)
                    child.OnContentClicked(button);
                    return;
                }
            }

            // Click wasn't on this node nor any visible child -> ignore.
            return;
        }

        var now = DateTime.UtcNow;
        var nowSec = (now - DateTime.UnixEpoch).TotalSeconds;

        // Get the root for this tree so clicks are isolated per-tree.
        var root = GetRoot();

        // If a descendant in the same root was just clicked, ignore bubbling here.
        if (GlobalLastClickByRoot.TryGetValue(root, out var entry) &&
            entry.node != this &&
            IsAncestorOf(entry.node) &&
            nowSec - entry.time < 0.05)
        {
            return;
        }

        // Mark this node as the most-recent click target for this root.
        GlobalLastClickByRoot[root] = (this, nowSec);

        // Existing double-click / single-click behaviour
        if (lastClickTime > 0 && nowSec - lastClickTime <= Style.DoubleClickSeconds)
        {
            // double click detected
            lastClickTime = -1;
            pendingSingleClickUntil = -1;

            DoubleClickInvocation?.Invoke(this);
            ToggleCollapsed();
        }
        else
        {
            lastClickTime = nowSec;
            pendingSingleClickUntil = nowSec + Style.DoubleClickSeconds;
        }
    }

    protected internal override void OnClickedOutsideOfContent(MouseButton button)
    {
        // Delegate to children so they can react to outside clicks as needed
        foreach (var child in Children)
        {
            child.OnClickedOutsideOfContent(button);
        }
    }

    protected internal override void OnHover()
    {
        // Delegate hover to children (they can decide to react or ignore)
        foreach (var child in Children)
        {
            child.OnHover();
        }
    }

    protected internal override void OnHoverEnd()
    {
        // Delegate hover-end to children
        foreach (var child in Children)
        {
            child.OnHoverEnd();
        }
    }

    protected internal override void OnOwnerClosing()
    {
        // If this is the root node, remove any stored click state for this root.
        if (Parent == null)
        {
            GlobalLastClickByRoot.Remove(this);
        }

        // Let children clean up first
        foreach (var child in Children)
        {
            child.OnOwnerClosing();

            // sever parent link for TreeNode children
            if (child is UITreeNode treeChild)
                treeChild.Parent = null;
        }

        // clear tracking structures
        Children.Clear();
        ChildLastRowRects.Clear();
    }
}
