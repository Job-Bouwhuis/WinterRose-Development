using Raylib_cs;
using System;
using System.Numerics;
using WinterRose;
using WinterRose.ForgeWarden;
using WinterRose.ForgeWarden.DialogBoxes.Boxes;
using WinterRose.ForgeWarden.TextRendering;

namespace WinterRose.ForgeWarden.DialogBoxes
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

        public static Dialog Show(Dialog dialog)
        {
            Rectangle newBounds = GetDialogBounds(dialog.Placement);
            bool placementConflict = activeDialogs.Any(d => ray.CheckCollisionRecs(GetDialogBounds(d.Placement), newBounds));

            if (placementConflict)
                queuedDialogs.Enqueue(dialog);
            else
            {
                activeDialogs.Add(dialog);
            }

            return dialog;
        }

        public static void Update(float deltaTime)
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
                            if (pending.Priority > oc.Priority)
                            {
                                oc.IsClosing = true;
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
                            pending.AnimationTimeIn = 0;
                            pending.AnimationTimeOut = 0;
                            pending.ContentAlphaTime = 0;
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
                        pending.AnimationTimeIn = 0;
                        pending.AnimationTimeOut = 0;
                        pending.ContentAlphaTime = 0;
                    }
                }

                queuedDialogs = requeue;
            }

            for (int i = activeDialogs.Count - 1; i >= 0; i--)
            {
                Dialog dialog = activeDialogs[i];

                if (!dialog.IsClosing)
                {
                    dialog.AnimationTimeIn = MathF.Min(dialog.AnimationTimeIn + deltaTime, ANIMATION_DURATION);
                    if (dialog.AnimationTimeIn / ANIMATION_DURATION >= 0.9f)
                    {
                        dialog.ContentAlphaTime = MathF.Min(dialog.ContentAlphaTime + deltaTime, CONTENT_FADE_DURATION);
                        dialog.YAnimateTime = MathF.Min(dialog.YAnimateTime + deltaTime, CONTENT_Y_MOVE_DURATION);
                    }

                    dialog.UpdateBox();
                }
                else
                {
                    dialog.AnimationTimeOut += deltaTime;
                    dialog.ContentAlphaTime = MathF.Max(dialog.ContentAlphaTime - deltaTime * 2f, 0f);
                    if (dialog.AnimationTimeOut >= ANIMATION_DURATION)
                    {
                        activeDialogs.RemoveAt(i);
                        if(dialog.WasBumped)
                        {
                            // If the dialog was bumped, we don't want to requeue it
                            dialog.WasBumped = false;
                            queuedDialogs = new Queue<Dialog>(new[] { dialog }.Concat(queuedDialogs));
                        }
                        else
                        {
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
                        pending.AnimationTimeIn = 0;
                        pending.AnimationTimeOut = 0;
                        pending.ContentAlphaTime = 0;
                    }
                    else
                        requeue.Enqueue(pending);
                }

                queuedDialogs = requeue;
            }

        }

        public static void Draw()
        {
            for (int i = 0; i < activeDialogs.Count; i++)
            {
                Dialog dialog = activeDialogs[i];
                Rectangle bounds = GetDialogBounds(dialog.Placement);

                float alphaT = dialog.IsClosing
                    ? MathF.Min(dialog.AnimationTimeOut / (ANIMATION_DURATION * ANIM_SPEED_ALPHA), 1f)
                    : MathF.Min(dialog.AnimationTimeIn / (ANIMATION_DURATION * ANIM_SPEED_ALPHA), 1f);

                float widthT = dialog.IsClosing
                    ? MathF.Min(dialog.AnimationTimeOut / (ANIMATION_DURATION * ANIM_SPEED_SCALE_WIDTH), 1f)
                    : MathF.Min(dialog.AnimationTimeIn / (ANIMATION_DURATION * ANIM_SPEED_SCALE_WIDTH), 1f);

                float heightT = dialog.IsClosing
                    ? MathF.Min(dialog.AnimationTimeOut / (ANIMATION_DURATION * ANIM_SPEED_SCALE_HEIGHT), 1f)
                    : MathF.Min(dialog.AnimationTimeIn / (ANIMATION_DURATION * ANIM_SPEED_SCALE_HEIGHT), 1f);

                float easedAlpha = 1 - MathF.Pow(1 - alphaT, 3);
                float easedWidth = 1 - MathF.Pow(1 - widthT, 3);
                float easedHeight = 1 - MathF.Pow(1 - heightT, 3);

                float eased = (easedAlpha + easedWidth + easedHeight) / 3;

                float alpha = dialog.IsClosing ? 1f - easedAlpha : easedAlpha;
                float scaleWidth = dialog.IsClosing ? 1f - easedWidth : easedWidth;
                float scaleHeight = dialog.IsClosing ? 1f - easedHeight : easedHeight;

                Vector2 center = new Vector2(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
                float drawWidth = bounds.Width * scaleWidth;
                float drawHeight = bounds.Height * scaleHeight;
                Rectangle scaled = new Rectangle(center.X - drawWidth / 2, center.Y - drawHeight / 2, drawWidth, drawHeight);

                Color bgColor = new Color(30, 30, 30, (int)(180 * alpha));
                Color borderColor = new Color(200, 200, 200, (int)(255 * alpha));
                Color shadowColor = new Color(0, 0, 0, (int)(80 * alpha));

                ray.DrawRectangle((int)scaled.X + 4, (int)scaled.Y + 4, (int)scaled.Width, (int)scaled.Height, shadowColor);
                ray.DrawRectangleRec(scaled, bgColor);
                ray.DrawRectangleLinesEx(scaled, 2, borderColor);

                bool drawInternals = !dialog.IsClosing && eased >= 0.9f;
                if (!drawInternals)
                    continue;

                float contentFadeT = Math.Clamp(dialog.ContentAlphaTime / CONTENT_FADE_DURATION, 0f, 1f);
                float contentAlpha = contentFadeT * alpha;
                dialog.Style.contentAlpha = contentAlpha;

                float contentYT = Math.Clamp(dialog.ContentAlphaTime / CONTENT_Y_MOVE_DURATION, 0f, 1f);
                int padding = 20;
                float innerWidth = scaled.Width - padding * 2;
                float contentOffsetY = (1f - contentYT) * 10f;
                int y = (int)(scaled.Y + padding + contentOffsetY);

                dialog.RenderBox(scaled, contentAlpha, ref padding, ref innerWidth, ref y);
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
            foreach(var d in activeDialogs)
                d.Close();
            if(includeQueued)
                queuedDialogs.Clear();
        }

        internal static List<Dialog> GetActiveDialogs() => activeDialogs;
    }
}