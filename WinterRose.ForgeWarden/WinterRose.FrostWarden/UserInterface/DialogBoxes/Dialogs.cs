using Raylib_cs;
using System;
using System.Diagnostics.Tracing;
using System.Numerics;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes.Boxes;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes.Enums;

namespace WinterRose.ForgeWarden.UserInterface.DialogBoxes
{
    public static class Dialogs
    {
        internal const float ANIMATION_DURATION = 0.45f;
        internal const float ANIM_SPEED_ALPHA = 0.35f;
        internal const float ANIM_SPEED_SCALE_WIDTH = 0.8f;
        internal const float ANIM_SPEED_SCALE_HEIGHT = 1.25f;
        internal const float CONTENT_FADE_DURATION = 0.25f;
        internal const float CONTENT_Y_MOVE_DURATION = 0.2f;

        private static List<Dialog> activeDialogs = new List<Dialog>();
        private static Queue<Dialog> queuedDialogs = new Queue<Dialog>();

        public static int OpenDialogs => activeDialogs.Count;
        public static int QueuedDialogs => queuedDialogs.Count;
        public static int TotalDialogs => OpenDialogs + QueuedDialogs;

        internal static InputContext Input { get; }

        static Dialogs()
        {
            Input = new InputContext(new RaylibInputProvider(), 2);
        }

        public static DialogShowState Show(Dialog dialog)
        {
            Rectangle newBounds = GetDialogBounds(dialog.Placement);
            bool placementConflict = activeDialogs.Any(d => ray.CheckCollisionRecs(GetDialogBounds(d.Placement), newBounds));

            if (activeDialogs.Contains(dialog))
                return DialogShowState.Active;
            if (queuedDialogs.Contains(dialog))
                return DialogShowState.Queued;

            dialog.CurrentAnim = new DialogAnimation();
            dialog.IsClosing = false;

            if (placementConflict)
            {
                queuedDialogs.Enqueue(dialog);
                return DialogShowState.Queued;
            }
            activeDialogs.Add(dialog);
            return DialogShowState.Active;
        }

        public static void Update(float deltaTime)
        {
            HandlePriorityDialogs();

            for (int i = activeDialogs.Count - 1; i >= 0; i--)
            {
                Dialog dialog = activeDialogs[i];

                if (dialog.DrawContentOnly)
                    continue;

                if (!dialog.IsClosing)
                {
                    UpdateDialogAnimation(dialog, deltaTime);
                    dialog.UpdateContainer();
                }
                else
                {
                    UpdateDialogAnimation(dialog, deltaTime);
                    if (dialog.CurrentAnim.Completed)
                    {
                        activeDialogs.RemoveAt(i);
                        if (dialog.WasBumped)
                        {
                            // If the dialog was bumped, we don't want to requeue it
                            dialog.WasBumped = false;
                            queuedDialogs = new Queue<Dialog>(new[] { dialog }.Concat(queuedDialogs));
                        }
                        continue;
                    }
                }
            }

            // Try activating queued dialogs if their placement is now free
            if (queuedDialogs.Count > 0)
            {
                Queue<Dialog> requeue = new Queue<Dialog>();

                while (queuedDialogs.Count > 0)
                {
                    Dialog pending = queuedDialogs.Dequeue();
                    Rectangle pendingBounds = GetDialogBounds(pending.Placement);

                    bool overlaps = activeDialogs.Any(active =>
                    {
                        if (active.IsClosing)
                            return false;
                        Rectangle activeBounds = GetDialogBounds(active.Placement);
                        return ray.CheckCollisionRecs(pendingBounds, activeBounds);
                    });

                    if (!overlaps)
                    {
                        activeDialogs.Add(pending);
                        pending.IsClosing = false;
                        pending.CurrentAnim = pending.CurrentAnim with
                        {
                            Elapsed = 0,
                            Completed = false
                        };
                    }
                    else
                        requeue.Enqueue(pending);
                }

                queuedDialogs = requeue;
            }

        }

        internal static bool IsDialogSpotAvailible(DialogPlacement placement) => activeDialogs
                        .Where(d => !d.IsClosing && ray.CheckCollisionRecs(GetDialogBounds(d.Placement), GetDialogBounds(placement)))
                        .ToArray().Length == 0;

        private static void HandlePriorityDialogs()
        {
            if (queuedDialogs.Count > 0)
            {
                Queue<Dialog> requeue = new Queue<Dialog>();

                while (queuedDialogs.Count > 0)
                {
                    Dialog pending = queuedDialogs.Dequeue();
                    Rectangle newBounds = GetDialogBounds(pending.Placement);

                    var occupying = activeDialogs
                        .Where(d => !d.IsClosing && ray.CheckCollisionRecs(GetDialogBounds(d.Placement), newBounds))
                        .ToArray();

                    if (occupying.Length > 0)
                    {
                        bool allowAdd = true;

                        foreach (var oc in occupying)
                        {
                            if (pending.Priority > oc.Priority && !oc.DrawContentOnly)
                            {
                                oc.Close();
                                oc.WasBumped = true;
                            }
                            else
                            {
                                allowAdd = false;
                                break;
                            }
                        }

                        if (allowAdd)
                        {
                            activeDialogs.Add(pending);
                            pending.IsClosing = false;
                            pending.CurrentAnim = pending.CurrentAnim with
                            {
                                Elapsed = 0,
                                Completed = false
                            };
                        }
                        else
                        {
                            requeue.Enqueue(pending);
                        }
                    }
                    else
                    {
                        activeDialogs.Add(pending);
                        pending.IsClosing = false;
                        pending.CurrentAnim = pending.CurrentAnim with
                        {
                            Elapsed = 0,
                            Completed = false
                        };
                    }
                }

                queuedDialogs = requeue;
            }
        }

        static void UpdateDialogAnimation(Dialog dialog, float deltaTime)
        {
            DialogAnimation anim = dialog.CurrentAnim;
            anim.Elapsed += deltaTime;

            var style = dialog.Style;

            float duration = dialog.IsClosing ? style.AnimateOutDuration : style.AnimateInDuration;
            float elapsed = anim.Elapsed;
            float t = Math.Clamp(elapsed / duration, 0f, 1f);

            float width = 1f, height = 1f, alpha = 1f;

            float alphaT = t;

            if (dialog.IsClosing)
            {
                alphaT *= 2f;
                if (alphaT > 1f) alphaT = 1f;
            }

            width = style.MoveAndScaleCurve?.Evaluate(t) ?? t;
            height = style.MoveAndScaleCurve?.Evaluate(t) ?? t;
            alpha = style.AlphaCurve?.Evaluate(alphaT) ?? alphaT;


            if (dialog.IsClosing)
            {
                width = 1f - width;
                height = 1f - height;
                alpha = 1f - alpha;
            }


            anim.Alpha = alpha;
            anim.ScaleWidth = width;
            anim.ScaleHeight = height;
            anim.Completed = t >= 1f; // mark completion
            dialog.CurrentAnim = anim;
        }


        public static void Draw()
        {
            int hoverCount = 0;

            for (int i = 0; i < activeDialogs.Count; i++)
            {
                Dialog dialog = activeDialogs[i];
                Rectangle bounds = GetDialogBounds(dialog.Placement);

                // Use the animation state calculated in UpdateDialogAnimation
                float alpha = dialog.CurrentAnim.Alpha;
                float scaleWidth = dialog.CurrentAnim.Completed ? 1 : dialog.CurrentAnim.ScaleWidth;
                float scaleHeight = dialog.CurrentAnim.Completed ? 1 : dialog.CurrentAnim.ScaleHeight;

                // Center and scale the dialog box
                Vector2 center = new Vector2(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
                float drawWidth = Math.Max(bounds.Width * scaleWidth, 0);
                float drawHeight = Math.Max(bounds.Height * scaleHeight, 0);
                Rectangle scaled = new Rectangle(center.X - drawWidth / 2, center.Y - drawHeight / 2, drawWidth, drawHeight);
                if (drawWidth == 0 && drawHeight == 0)
                    continue;

                float shadowX = scaled.X - dialog.Style.ShadowSizeLeft;
                float shadowY = scaled.Y - dialog.Style.ShadowSizeTop;
                float shadowWidth = scaled.Width + dialog.Style.ShadowSizeLeft + dialog.Style.ShadowSizeRight;
                float shadowHeight = scaled.Height + dialog.Style.ShadowSizeTop + dialog.Style.ShadowSizeBottom;

                if (!dialog.DrawContentOnly)
                {
                    ray.DrawRectangle((int)shadowX, (int)shadowY, (int)shadowWidth, (int)shadowHeight, dialog.Style.Shadow);
                    ray.DrawRectangleRec(scaled, dialog.Style.Background);
                    ray.DrawRectangleLinesEx(scaled, 2, dialog.Style.Border);
                }

                // Only draw internals when dialog is mostly visible
                bool drawInternals = alpha >= (dialog.IsClosing ? 0.3f : 0.7f);
                if (!drawInternals)
                    continue;

                if (!dialog.IsClosing && Input.HighestPriorityMouseAbove is null)
                {
                    var mousepos = Input.Provider.MousePosition;
                    if (ray.CheckCollisionPointRec(mousepos, scaled))
                        hoverCount++;
                }

                // Content fade & Y-move driven by style durations
                float contentFadeT = Math.Clamp((dialog.Style.AlphaSpeed += Time.deltaTime) / dialog.Style.ContentFadeDuration, 0f, 1f);
                float contentAlpha = contentFadeT * alpha;
                dialog.Style.ContentAlpha = contentAlpha;

                dialog.CurrentPosition = scaled;

                dialog.Draw();
            }

            if (hoverCount > 0)
            {
                Input.IsRequestingMouseFocus = true;
            }
            else
            {
                Input.IsRequestingMouseFocus = false;
            }
        }


        internal static Rectangle GetDialogBounds(DialogPlacement placement)
        {
            int screenWidth = ray.GetScreenWidth();
            int screenHeight = ray.GetScreenHeight();

            const float SMALL_PAD = 0.25f;

            float smallWidth = screenWidth * SMALL_PAD;
            float smallHeight = screenHeight * SMALL_PAD;

            return placement switch
            {
                // center
                DialogPlacement.CenterSmall => new Rectangle(
                    screenWidth * SMALL_PAD,
                    screenHeight * SMALL_PAD,
                    screenWidth * (1 - SMALL_PAD * 2),
                    screenHeight * (1 - SMALL_PAD * 2)),

                DialogPlacement.CenterBig => new Rectangle(0, 0, screenWidth, screenHeight),


                // left
                DialogPlacement.LeftSmall => new Rectangle(0,
                    screenHeight * SMALL_PAD,
                    smallWidth,
                    screenHeight * (1 - SMALL_PAD * 2)),

                DialogPlacement.LeftBig => new Rectangle(0, 0, smallWidth, screenHeight),


                // right
                DialogPlacement.RightSmall => new Rectangle(
                    screenWidth * (1 - SMALL_PAD),
                    screenHeight * SMALL_PAD,
                    smallWidth,
                    screenHeight * (1 - SMALL_PAD * 2)),

                DialogPlacement.RightBig => new Rectangle(screenWidth - smallWidth, 0, smallWidth, screenHeight),

                // top
                DialogPlacement.TopSmall => new Rectangle(screenWidth * SMALL_PAD, 0,
                    screenWidth * (1 - SMALL_PAD * 2),
                    smallHeight),

                DialogPlacement.TopBig => new Rectangle(0, 0, screenWidth, smallHeight),

                // bottom
                DialogPlacement.BottomSmall => new Rectangle(screenWidth * SMALL_PAD,
                    screenHeight - smallHeight,
                    screenWidth * (1 - SMALL_PAD * 2),
                    smallHeight),

                DialogPlacement.BottomBig => new Rectangle(0,
                    screenHeight - smallHeight,
                    screenWidth,
                    smallHeight),

                // corners
                DialogPlacement.TopLeft => new Rectangle(0, 0, smallWidth, smallHeight),

                DialogPlacement.TopRight => new Rectangle(screenWidth - smallWidth, 0, smallWidth, smallHeight),

                DialogPlacement.BottomLeft => new Rectangle(0, screenHeight - smallHeight, smallWidth, smallHeight),

                DialogPlacement.BottomRight => new Rectangle(screenWidth - smallWidth, screenHeight - smallHeight, smallWidth, smallHeight),

                // more center ones
                DialogPlacement.HorizontalBig => new Rectangle(
                    0,
                    screenHeight * SMALL_PAD,
                    screenWidth,
                    screenHeight * (1 - SMALL_PAD * 2)),

                DialogPlacement.VerticalBig => new Rectangle(
                    screenWidth * SMALL_PAD,
                    0,
                    screenWidth * (1 - SMALL_PAD * 2),
                    screenHeight),


                _ => new Rectangle(
                    screenWidth / 2 - 200,
                    screenHeight / 2 - 100,
                    400,
                    200),
            };
        }

        public static void CloseAll(bool includeQueued = false)
        {
            foreach (var d in activeDialogs)
                d.Close();
            if (includeQueued)
                queuedDialogs.Clear();
        }

        internal static List<Dialog> GetActiveDialogs() => activeDialogs;
        internal static void AddImmediately(Dialog dialog)
        {
            activeDialogs.Add(dialog);
        }
    }
}