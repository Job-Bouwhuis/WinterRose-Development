using Raylib_cs;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.UserInterface.Tooltipping.Anchors;
using WinterRose.ForgeWarden.UserInterface.Tooltipping.Behaviors;

namespace WinterRose.ForgeWarden.UserInterface.Tooltipping
{
    public static class Tooltips
    {
        private const int PRIORITY_BASE = 200000;
        private const int PRIORITY_RANGE = 5000;

        private static readonly Dictionary<UIContent, HashSet<Tooltip>> hoverExtenders = new();

        private static readonly List<Tooltip> activeTooltips = new List<Tooltip>();

        public static Tooltip ForUIContent(UIContent content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            var anchor = new UIContentAnchor(content);
            var behavior = new UIContentAnchoredBehavior();

            return new Tooltip(behavior, anchor);
        }

        public static Tooltip Show(Tooltip tooltip) => Show(tooltip, tooltip.Anchor, tooltip.Behavior);

        public static Tooltip Show(Tooltip tooltip, TooltipAnchor anchor, TooltipBehavior behavior)
        {
            if (tooltip == null) throw new ArgumentNullException(nameof(tooltip));
            if (anchor == null) throw new ArgumentNullException(nameof(anchor));
            if (behavior == null) throw new ArgumentNullException(nameof(behavior));

            tooltip.Anchor = anchor;
            tooltip.Behavior = behavior;

            // ask behavior if it should open
            if (!behavior.ShouldOpen(tooltip))
                return tooltip;

            // ask override
            if (tooltip.OpenOverride != null && !tooltip.OpenOverride(tooltip))
                return tooltip;

            if (!activeTooltips.Contains(tooltip))
            {
                InputManager.RegisterContext(tooltip.Input);
                activeTooltips.Add(tooltip);
                ReassignPriorities();

                // measure size and position first time
                ComputeSizeAndPositionForTooltip(tooltip);
                tooltip.IsOpen = true;
            }

            return tooltip;
        }

        public static void Close(Tooltip tooltip) => Close(tooltip, TooltipCloseReason.Explicit);

        internal static void Close(Tooltip tooltip, TooltipCloseReason reason)
        {
            if (tooltip == null) return;

            // allow override to block closure
            if (tooltip.CloseOverride != null && !tooltip.CloseOverride(tooltip, reason))
                return;

            if (!tooltip.Behavior.ShouldClose(tooltip, reason))
                return;

            UnregisterAllHoverExtendersForTooltip(tooltip);

            if (activeTooltips.Remove(tooltip))
                InputManager.UnregisterContext(tooltip.Input);
        }

        internal static void Update()
        {
            for (int i = activeTooltips.Count - 1; i >= 0; i--)
            {
                Tooltip t = activeTooltips[i];

                if (!t.Anchor.IsAnchorValid())
                {
                    Close(t, TooltipCloseReason.TargetHoverLost);
                    continue;
                }

                // reposition or update layout if needed
                ComputeSizeAndPositionForTooltip(t);

                // always update even when closing so that animation can run
                t.UpdateContainer();
            }
        }

        internal static void Draw()
        {
            for (int i = 0; i < activeTooltips.Count; i++)
                activeTooltips[i].Draw();
        }

        internal static void RequestReposition(Tooltip tooltip)
        {
            ComputeSizeAndPositionForTooltip(tooltip);
        }

        private static void ComputeSizeAndPositionForTooltip(Tooltip tooltip)
        {
            // resolve size using layout resolver
            Vector2 chosenSize = TooltipLayoutResolver.ResolveBestSize(tooltip, tooltip.SizeConstraints);

            tooltip.TargetSize = chosenSize;
            tooltip.AnimationElapsed = 0f;

            // compute placement
            Rectangle anchorRect = tooltip.Anchor.GetAnchorBounds();

            if (tooltip.Behavior.Mode == TooltipMode.FollowMouse && tooltip.Anchor is UIContainerTooltipAnchor)
            {
                var fb = tooltip.Behavior as FollowMouseTooltipBehavior;
                Vector2 mp = tooltip.Input.MousePosition;
                Vector2 offset = fb?.MouseOffset ?? new Vector2(12f, 18f);

                Vector2 pos = new Vector2(mp.X + offset.X, mp.Y + offset.Y);

                // clamp to viewport (assumes a global ScreenWidth/ScreenHeight)
                pos = ClampPositionToViewport(pos, chosenSize);

                tooltip.TargetPosition = pos;
            }
            else
            {
                // static placement: prefer bottom‑right of anchor
                Vector2 pos = new Vector2(anchorRect.X + anchorRect.Width, anchorRect.Y + anchorRect.Height);

                pos = ClampPositionToViewport(pos, chosenSize);

                tooltip.TargetPosition = pos;
            }
        }

        private static Vector2 ClampPositionToViewport(Vector2 position, Vector2 size)
        {
            float screenW = ForgeWardenEngine.Current.Window.Width;
            float screenH = ForgeWardenEngine.Current.Window.Height;

            float x = position.X;
            float y = position.Y;

            if (x + size.X > screenW) x = Math.Max(0f, screenW - size.X - 8f);
            if (y + size.Y > screenH) y = Math.Max(0f, screenH - size.Y - 8f);

            return new Vector2(x, y);
        }

        private static void ReassignPriorities()
        {
            int count = activeTooltips.Count;

            for (int i = 0; i < count; i++)
            {
                int priority = PRIORITY_BASE + (i % PRIORITY_RANGE);
                activeTooltips[i].Input.Priority = priority;
            }
        }

        public static void RegisterHoverExtender(UIContent content, Tooltip tooltip)
        {
            if (content == null || tooltip == null) return;

            if (!hoverExtenders.TryGetValue(content, out var set))
            {
                set = new HashSet<Tooltip>();
                hoverExtenders[content] = set;
            }

            set.Add(tooltip);
        }

        public static void UnregisterHoverExtender(UIContent content, Tooltip tooltip)
        {
            if (content == null || tooltip == null) return;

            if (!hoverExtenders.TryGetValue(content, out var set))
                return;

            set.Remove(tooltip);
            if (set.Count == 0)
                hoverExtenders.Remove(content);
        }
        public static bool IsHoverExtended(UIContent content)
        {
            if (content == null) return false;
            return hoverExtenders.ContainsKey(content);
        }
        public static void UnregisterAllHoverExtendersForTooltip(Tooltip tooltip)
        {
            if (tooltip == null) return;

            var toRemove = new List<UIContent>();

            foreach (var kv in hoverExtenders)
            {
                if (kv.Value.Remove(tooltip))
                {
                    if (kv.Value.Count == 0)
                        toRemove.Add(kv.Key);
                }
            }

            foreach (var key in toRemove)
                hoverExtenders.Remove(key);
        }
    }
}