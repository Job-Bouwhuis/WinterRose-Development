using Raylib_cs;
using System.Runtime.CompilerServices;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.UserInterface.Tooltipping.Anchors;
using WinterRose.ForgeWarden.UserInterface.Tooltipping.Behaviors;
using WinterRose.Recordium;

namespace WinterRose.ForgeWarden.UserInterface.Tooltipping
{
    public static class Tooltips
    {
        private const int PRIORITY_BASE = 200000;
        private const int PRIORITY_RANGE = 5000;

        private static Log log = new Log("Tooltips");

        private static readonly Dictionary<UIContent, HashSet<Tooltip>> hoverExtenders = new();

        private static readonly List<Tooltip> activeTooltips = new List<Tooltip>();

        public static Tooltip ForUIContent(UIContent content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            var anchor = new UIContentAnchor(content);
            var behavior = new UIContentAnchoredBehavior();

            return new Tooltip(behavior, anchor);
        }
        public static Tooltip Static(Vector2 position, Vector2 size)
        {
            var anchor = new StaticPositionAnchor(position, size);
            var behavior = new StaticPositionBehavior();
            return new Tooltip(behavior, anchor);
        }

        public static Tooltip MouseFollow(Vector2 size)
        {
            var anchor = new FollowMouseAnchor(size);
            var behavior = new FollowMouseBehavior();
            return new Tooltip(behavior, anchor) { SizeConstraints = { MinSize = size, MaxSize = size } };
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
                activeTooltips.Add(tooltip);
                ReassignPriorities();

                // measure size and position first time
                ComputeSizeAndPositionForTooltip(tooltip);
                tooltip.IsOpen = true;

                Vector2 targetPos = tooltip.TargetPosition;
                Vector2 targetSize = tooltip.TargetSize;

                tooltip.CurrentPosition = new Rectangle(
                    x: targetPos.X + targetSize.X / 2,
                    y: targetPos.Y + targetSize.Y / 2,
                    width: 0,
                    height: 0);


                ForgeWardenEngine.Current.GlobalThreadLoom.InvokeAfter(
                    ForgeWardenEngine.ENGINE_POOL_NAME,
                    () => BringToFront(tooltip),
                    TimeSpan.FromMilliseconds(50));
            }
            else
            {
                BringToFront(tooltip);
            }

            //tooltip.AddContent(new UIText("")
            //{
            //    TextProvider = () => tooltip.Input.Priority.ToString()
            //});
            //tooltip.AddContent(new UIText("")
            //{
            //    TextProvider = () => InputManager.IsRegistered(tooltip.Input).ToString()
            //});
            return tooltip;
        }

        private static void BringToFront(Tooltip tooltip)
        {
            int idx = activeTooltips.IndexOf(tooltip);
            if (idx < 0) return;

            if (idx != activeTooltips.Count - 1)
            {
                activeTooltips.RemoveAt(idx);
                activeTooltips.Add(tooltip);
                ReassignPriorities();
            }
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

            tooltip.Close();
        }

        internal static void ForceRemoveTooltip(Tooltip tooltip)
        {
            if (tooltip == null) return;

            if (activeTooltips.Remove(tooltip))
            {
                InputManager.UnregisterContext(tooltip.Input);
                ReassignPriorities();
            }
        }

        internal static void Update()
        {
            for (int i = activeTooltips.Count - 1; i >= 0; i--)
            {
                Tooltip t = activeTooltips[i];

                if (!t.Anchor.IsAnchorValid(t.IsHovered()) && !t.IsClosing)
                {
                    Close(t, TooltipCloseReason.TargetHoverLost);
                    continue;
                }

                if (!t.IsClosing)
                    ComputeSizeAndPositionForTooltip(t);

                t.UpdateContainer();
            }
        }

        internal static void Draw()
        {
            for (int i = 0; i < activeTooltips.Count; i++)
                activeTooltips[i].Draw();
        }

        private static void ComputeSizeAndPositionForTooltip(Tooltip tooltip)
        {
            if (tooltip.IsClosing)
                return;

            Vector2 chosenSize = TooltipLayoutResolver.ResolveBestSize(tooltip, tooltip.SizeConstraints);
            Rectangle anchorRect = tooltip.Anchor.GetAnchorBounds();
            Vector2 pos = new Vector2(anchorRect.X, anchorRect.Y);
            pos = ClampPositionToViewport(pos, chosenSize);

            if (NearlyEqual(pos, tooltip.TargetPosition, 1))
                return;
            if (NearlyEqual(chosenSize, tooltip.TargetSize, 1))
                return;

            log.Debug($"Tooltip position changed: {tooltip.TargetPosition} -> {pos}, size changed: {tooltip.TargetSize} -> {chosenSize}");
            tooltip.TargetPosition = pos;
            tooltip.TargetSize = chosenSize;
            tooltip.AnimationElapsed = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool NearlyEqual(Vector2 a, Vector2 b, float tolerance) => Vector2.DistanceSquared(a, b) <= tolerance * tolerance;

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

            List<Tooltip> mouseAnchors = new(count);
            List<Tooltip> nonInteractable = new(count);
            List<Tooltip> normal = new(count);

            for (int i = 0; i < count; i++)
            {
                Tooltip t = activeTooltips[i];
                if (t.Anchor is FollowMouseAnchor)
                    mouseAnchors.Add(t);
                else if (!t.Behavior.AllowsInteraction)
                    nonInteractable.Add(t);
                else
                    normal.Add(t);
            }

            // clear original list
            activeTooltips.Clear();

            // lower in list = higher priority, so we add normal first, then non-interactable, then mouse-anchored
            activeTooltips.AddRange(normal);
            activeTooltips.AddRange(nonInteractable);
            activeTooltips.AddRange(mouseAnchors);

            // assign priorities
            int currentPriority = PRIORITY_BASE;
            for (int i = 0; i < activeTooltips.Count; i++)
            {
                // lower index = higher priority, so the last elements (mouseAnchors) get highest
                if (activeTooltips[i] is null)
                {
                    activeTooltips.RemoveAt(i--);
                    continue;
                }
                activeTooltips[i].Input.Priority = currentPriority + (i % PRIORITY_RANGE);
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
            bool r = hoverExtenders.ContainsKey(content);
            if (r)
                return true;

            if (content is IUIContainer cont)
            {
                for (int i = 0; i < cont.Contents.Count; i++)
                {
                    UIContent? child = cont.Contents[i];
                    if (IsHoverExtended(child))
                        return true;
                }
            }

            return false;
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