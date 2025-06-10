using Raylib_cs;
using System;
using System.Numerics;
using WinterRose.FrostWarden;
using WinterRose.FrostWarden.TextRendering;

public enum DialogType
{
    ConfirmCancel,
    YesNo,
    RetryCancel,
    Timed,
    Progress,
    ImGui
}

public enum DialogPlacement
{
    CenterSmall,
    CenterBig,

    LeftSmall,
    LeftBig,

    RightSmall,
    RightBig,

    TopSmall,
    TopBig,

    BottomSmall,
    BottomBig
}

public static class DialogBox
{
    public class DialogInstance
    {
        public RichText Title { get; set; }
        public RichText Message { get; set; }
        public DialogType Type { get; internal set; }
        public DialogPlacement Placement { get; set; }

        private float progressValue;
        public float ProgressValue
        {
            get => progressValue;
            set => progressValue = Math.Clamp(value, 0f, 1f);
        }

        public float TimeRemaining { get; set; }

        public float AnimationTimeIn { get; internal set; }
        public float AnimationTimeOut { get; internal set; }
        public float ContentAlphaTime { get; internal set; } = 0f;

        public bool IsClosing { get; internal set; }

        public Action? OnConfirm { get; set; }
        public Action? OnCancel { get; set; }

        public Action<UIContext>? OnImGui { get; set; }

        public bool IsVisible => !IsClosing;

        public DialogInstance(string title, string message, DialogType type,
                              Action? onConfirm, Action? onCancel, Action<UIContext>? onImGui,
                              float progress, float timeRemaining,
                              DialogPlacement placement)
        {
            Title = RichText.Parse(title, Color.White);
            Message = RichText.Parse(message, Color.White);
            Type = type;
            Placement = placement;

            ProgressValue = progress;
            TimeRemaining = timeRemaining;

            AnimationTimeIn = 0f;
            AnimationTimeOut = 0f;
            ContentAlphaTime = 0f;

            IsClosing = false;

            OnConfirm = onConfirm;
            OnCancel = onCancel;
            OnImGui = onImGui;
        }

        public void Close()
        {
            IsClosing = true;
        }
    }

    private const float ANIMATION_DURATION = 0.5f;
    private const float ANIM_SPEED_ALPHA = 0.5f;         
    private const float ANIM_SPEED_SCALE_WIDTH = 1f;   
    private const float ANIM_SPEED_SCALE_HEIGHT = 0.5f; 
    const float CONTENT_FADE_DURATION = 0.55f;

    private static List<DialogInstance> activeDialogs = new List<DialogInstance>();
    private static Queue<DialogInstance> queuedDialogs = new Queue<DialogInstance>();

    public static DialogInstance Show(
        string titleText,
        string messageText,
        DialogType type,
        Action? confirm = null,
        Action? cancel = null,
        Action<UIContext>? onImGui = null,
        float progress = 0f,
        float durationSeconds = 0f,
        DialogPlacement placement = DialogPlacement.CenterSmall)
    {
        DialogInstance dialog = new DialogInstance(titleText, messageText, type, confirm, cancel, onImGui, progress, durationSeconds, placement);

        Rectangle newBounds = GetDialogBounds(placement);
        bool placementConflict = activeDialogs.Any(d => Raylib.CheckCollisionRecs(GetDialogBounds(d.Placement), newBounds));

        if (placementConflict)
        {
            queuedDialogs.Enqueue(dialog);
        }
        else
        {
            activeDialogs.Add(dialog);
        }

        return dialog;

    }

    public static void Update(float deltaTime)
    {
        for (int i = activeDialogs.Count - 1; i >= 0; i--)
        {
            DialogInstance dialog = activeDialogs[i];

            if (!dialog.IsClosing)
            {
                dialog.AnimationTimeIn = MathF.Min(dialog.AnimationTimeIn + deltaTime, ANIMATION_DURATION);
                if (dialog.AnimationTimeIn / ANIMATION_DURATION >= 0.9f)
                {
                    dialog.ContentAlphaTime = MathF.Min(dialog.ContentAlphaTime + deltaTime, CONTENT_FADE_DURATION);

                }
            }
            else
            {
                dialog.AnimationTimeOut += deltaTime;
                dialog.ContentAlphaTime = MathF.Max(dialog.ContentAlphaTime - deltaTime * 2f, 0f);
                if (dialog.AnimationTimeOut >= ANIMATION_DURATION)
                {
                    activeDialogs.RemoveAt(i);
                    continue;
                }
            }

            if (dialog.Type == DialogType.Timed && dialog.TimeRemaining > 0f)
            {
                dialog.TimeRemaining -= deltaTime;
                if (dialog.TimeRemaining <= 0f)
                {
                    dialog.IsClosing = true;
                    dialog.OnConfirm?.Invoke();
                }
            }
        }

        // Try activating queued dialogs if their placement is now free
        if (queuedDialogs.Count > 0)
        {
            Queue<DialogInstance> requeue = new Queue<DialogInstance>();
            while (queuedDialogs.Count > 0)
            {
                DialogInstance pending = queuedDialogs.Dequeue();
                if (activeDialogs.Any(d => d.Placement == pending.Placement))
                {
                    requeue.Enqueue(pending); // Still blocked
                }
                else
                {
                    activeDialogs.Add(pending);
                }
            }
            queuedDialogs = requeue;
        }
    }

    public static void Draw()
    {
        for (int i = 0; i < activeDialogs.Count; i++)
        {
            DialogInstance dialog = activeDialogs[i];
            Rectangle bounds = GetDialogBounds(dialog.Placement);

            // Calculate normalized animation times separately
            float alphaT = dialog.IsClosing
                ? MathF.Min(dialog.AnimationTimeOut / (ANIMATION_DURATION * ANIM_SPEED_ALPHA), 1f)
                : MathF.Min(dialog.AnimationTimeIn / (ANIMATION_DURATION * ANIM_SPEED_ALPHA), 1f);

            float widthT = dialog.IsClosing
                ? MathF.Min(dialog.AnimationTimeOut / (ANIMATION_DURATION * ANIM_SPEED_SCALE_WIDTH), 1f)
                : MathF.Min(dialog.AnimationTimeIn / (ANIMATION_DURATION * ANIM_SPEED_SCALE_WIDTH), 1f);

            float heightT = dialog.IsClosing
                ? MathF.Min(dialog.AnimationTimeOut / (ANIMATION_DURATION * ANIM_SPEED_SCALE_HEIGHT), 1f)
                : MathF.Min(dialog.AnimationTimeIn / (ANIMATION_DURATION * ANIM_SPEED_SCALE_HEIGHT), 1f);

            // Ease out lerps
            float easedAlpha = 1 - MathF.Pow(1 - alphaT, 3);
            float easedWidth = 1 - MathF.Pow(1 - widthT, 3);
            float easedHeight = 1 - MathF.Pow(1 - heightT, 3);

            float eased = (easedAlpha + easedWidth + easedHeight) / 3;

            // Apply easing and invert for closing
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

            Raylib.DrawRectangle((int)scaled.X + 4, (int)scaled.Y + 4, (int)scaled.Width, (int)scaled.Height, shadowColor);
            Raylib.DrawRectangleRec(scaled, bgColor);
            Raylib.DrawRectangleLinesEx(scaled, 2, borderColor);

            // Don’t draw internals until we’re past 90% of anim in
            bool drawInternals = !dialog.IsClosing && eased >= 0.9f;
            if (!drawInternals)
                continue;

            float contentFadeT = Math.Clamp(dialog.ContentAlphaTime / CONTENT_FADE_DURATION, 0f, 1f);
            float contentAlpha = contentFadeT * alpha;

            Color contentColor = new Color(255, 255, 255, (int)(255 * contentAlpha));
            Color fadedGray = new Color(180, 180, 180, (int)(255 * contentAlpha));
            Color barBackground = new Color(80, 80, 80, (int)(255 * contentAlpha));
            Color barFill = new Color(0, 150, 255, (int)(255 * contentAlpha));
            Color buttonTextColor = new Color(255, 255, 255, (int)(255 * contentAlpha));
            Color buttonBackground = new Color(100, 100, 100, (int)(255 * contentAlpha));
            Color timeLabelColor = new Color(150, 150, 150, (int)(255 * contentAlpha));

            float contentOffsetY = (1f - contentFadeT) * 10f;

            float padding = 20;
            float innerWidth = scaled.Width - padding * 2;
            int y = (int)(scaled.Y + padding + contentOffsetY);

            RichTextRenderer.DrawRichText(dialog.Title, new((int)scaled.X + (int)padding, y), null, 25, innerWidth, contentColor);
            y += 35;

            RichTextRenderer.DrawRichText(dialog.Message, new((int)scaled.X + (int)padding, y), null, 16, innerWidth, contentColor);
            y += 40;

            if (dialog.Type == DialogType.Progress)
            {
                Rectangle barBg = new Rectangle(scaled.X + padding, y, innerWidth, 20);
                Rectangle barFillRect = new Rectangle(barBg.X, barBg.Y, innerWidth * dialog.ProgressValue, 20);

                Raylib.DrawRectangleRec(barBg, barBackground);
                Raylib.DrawRectangleRec(barFillRect, barFill);

                string progressText = $"{MathF.Round(dialog.ProgressValue * 100f, 1)}%";
                int textWidth = Raylib.MeasureText(progressText, 14);
                int textX = (int)(barBg.X + (barBg.Width - textWidth) / 2);
                int textY = (int)(barBg.Y + 2); // a little padding from top

                Color progressTextColor = new Color(255, 255, 255, (int)(255 * contentAlpha));
                Raylib.DrawText(progressText, textX, textY, 14, progressTextColor);

                y += 30;
            }


            if (dialog.Type == DialogType.Timed)
            {
                string timeLabel = $"Closing in {MathF.Round(dialog.TimeRemaining, 1)}s";
                Raylib.DrawText(timeLabel, (int)scaled.X + (int)padding, y, 16, timeLabelColor);
                y += 25;
            }

            string[] labels = Array.Empty<string>();
            switch (dialog.Type)
            {
                case DialogType.ConfirmCancel: labels = new[] { "Confirm", "Cancel" }; break;
                case DialogType.YesNo: labels = new[] { "Yes", "No" }; break;
                case DialogType.RetryCancel: labels = new[] { "Retry", "Cancel" }; break;
            }

            float buttonWidth = 80;
            float buttonHeight = 30;
            float spacing = 10;
            float totalButtonWidth = labels.Length * buttonWidth + (labels.Length - 1) * spacing;
            float startX = scaled.X + (scaled.Width - totalButtonWidth) / 2;

            for (int j = 0; j < labels.Length; j++)
            {
                Rectangle btn = new Rectangle(startX + j * (buttonWidth + spacing), y, buttonWidth, buttonHeight);
                Raylib.DrawRectangleRec(btn, buttonBackground);
                Raylib.DrawText(labels[j], (int)(btn.X + 10), (int)(btn.Y + 7), 16, buttonTextColor);

                if (Raylib.IsMouseButtonPressed(MouseButton.Left) &&
                    Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), btn))
                {
                    dialog.IsClosing = true;
                    if (j == 0) dialog.OnConfirm?.Invoke();
                    else dialog.OnCancel?.Invoke();
                }
            }
            y += 25;

            UIContext c = new UIContext();
            c.Begin(new Vector2(scaled.X, y), contentColor);
            dialog.OnImGui?.Invoke(c);
            c.End();
        }

    }

    // Update GetDialogBounds to accept a parameter
    private static Rectangle GetDialogBounds(DialogPlacement placement)
    {
        int screenWidth = Raylib.GetScreenWidth();
        int screenHeight = Raylib.GetScreenHeight();

        const float SMALL_PAD = 0.25f;
        int minWidth = 400;
        int minHeight = 200;

        return placement switch
        {
            DialogPlacement.CenterSmall => new Rectangle(
                                screenWidth * SMALL_PAD,
                                screenHeight * SMALL_PAD,
                                screenWidth * (1 - SMALL_PAD * 2),
                                screenHeight * (1 - SMALL_PAD * 2)),
            DialogPlacement.CenterBig => new Rectangle(0, 0, screenWidth, screenHeight),

            DialogPlacement.LeftSmall => new Rectangle(0,
                                screenHeight * SMALL_PAD,
                                screenWidth * SMALL_PAD,
                                screenHeight * (1 - SMALL_PAD * 2)),
            DialogPlacement.LeftBig => new Rectangle(0, 0, screenWidth * SMALL_PAD, screenHeight),

            DialogPlacement.RightSmall => new Rectangle(
                                screenWidth * (1 - SMALL_PAD),
                                screenHeight * SMALL_PAD,
                                screenWidth * SMALL_PAD,
                                screenHeight * (1 - SMALL_PAD * 2)),
            DialogPlacement.RightBig => new Rectangle(screenWidth * (1 - SMALL_PAD), 0, screenWidth * SMALL_PAD, screenHeight),

            DialogPlacement.TopSmall => new Rectangle(screenWidth * SMALL_PAD, 0,
                                screenWidth * (1 - SMALL_PAD * 2),
                                screenHeight * SMALL_PAD),
            DialogPlacement.TopBig => new Rectangle(0, 0, screenWidth, screenHeight * SMALL_PAD),

            DialogPlacement.BottomSmall => new Rectangle(screenWidth * SMALL_PAD,
                                screenHeight * (1 - SMALL_PAD),
                                screenWidth * (1 - SMALL_PAD * 2),
                                screenHeight * SMALL_PAD),
            DialogPlacement.BottomBig => new Rectangle(0,
                                screenHeight * (1 - SMALL_PAD),
                                screenWidth,
                                screenHeight * SMALL_PAD),

            _ => new Rectangle(
                        screenWidth / 2 - minWidth / 2,
                        screenHeight / 2 - minHeight / 2,
                        minWidth,
                        minHeight),
        };
    }

}
