using Raylib_cs;
using System;
using System.Numerics;

public enum DialogType
{
    ConfirmCancel,
    YesNo,
    RetryCancel,
    Timed,
    Progress
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
    private static bool isVisible = false;
    private static string title = "";
    private static string message = "";
    private static DialogType currentType;
    private static DialogPlacement currentPlacement;

    private static float progressValue = 0f;
    private static float timeRemaining = 0f;
    private static float animationTime = 0f;
    private const float ANIMATION_DURATION = 0.25f;

    private static Action onConfirm;
    private static Action onCancel;

    public static void Show(
        string titleText,
        string messageText,
        DialogType type,
        Action confirm = null,
        Action cancel = null,
        float progress = 0f,
        float durationSeconds = 0f,
        DialogPlacement placement = DialogPlacement.CenterSmall)
    {
        isVisible = true;
        animationTime = 0f;

        title = titleText;
        message = messageText;
        currentType = type;
        currentPlacement = placement;

        onConfirm = confirm;
        onCancel = cancel;
        progressValue = progress;
        timeRemaining = durationSeconds;
    }

    public static void Update(float deltaTime)
    {
        if (!isVisible) return;

        animationTime = MathF.Min(animationTime + deltaTime, ANIMATION_DURATION);

        if (currentType == DialogType.Timed && timeRemaining > 0f)
        {
            timeRemaining -= deltaTime;
            if (timeRemaining <= 0f)
            {
                isVisible = false;
                onConfirm?.Invoke();
            }
        }
    }

    public static void Draw()
    {
        if (!isVisible) return;

        Rectangle bounds = GetDialogBounds();

        float alpha = animationTime / ANIMATION_DURATION;
        int borderAlpha = (int)(255 * alpha);
        int bgAlpha = (int)(180 * alpha);

        // Shadow
        Raylib.DrawRectangle((int)bounds.X + 4, (int)bounds.Y + 4, (int)bounds.Width, (int)bounds.Height, new Color(0, 0, 0, 80));
        // Background
        Raylib.DrawRectangleRec(bounds, new Color(30, 30, 30, bgAlpha));
        // Border
        Raylib.DrawRectangleLinesEx(bounds, 2, new Color(200, 200, 200, borderAlpha));

        float padding = 20;
        float innerWidth = bounds.Width - padding * 2;

        int y = (int)bounds.Y + (int)padding;

        Raylib.DrawText(title, (int)bounds.X + (int)padding, y, 20, Color.White);
        y += 30;

        Raylib.DrawText(message, (int)bounds.X + (int)padding, y, 16, Color.LightGray);
        y += 40;

        if (currentType == DialogType.Progress)
        {
            Rectangle barBg = new Rectangle(bounds.X + padding, y, innerWidth, 20);
            Rectangle barFill = new Rectangle(barBg.X, barBg.Y, innerWidth * progressValue, 20);
            Raylib.DrawRectangleRec(barBg, new Color(80, 80, 80, 255));
            Raylib.DrawRectangleRec(barFill, new Color(0, 150, 255, 255));
            y += 30;
        }

        if (currentType == DialogType.Timed)
        {
            string timeLabel = $"Closing in {MathF.Ceiling(timeRemaining)}s";
            Raylib.DrawText(timeLabel, (int)bounds.X + (int)padding, y, 16, Color.Gray);
            y += 25;
        }

        float buttonWidth = 80;
        float buttonHeight = 30;
        float spacing = 10;
        float totalButtonWidth = 0;

        int numButtons = 0;
        string[] labels = Array.Empty<string>();

        switch (currentType)
        {
            case DialogType.ConfirmCancel: labels = new[] { "Confirm", "Cancel" }; break;
            case DialogType.YesNo: labels = new[] { "Yes", "No" }; break;
            case DialogType.RetryCancel: labels = new[] { "Retry", "Cancel" }; break;
        }

        numButtons = labels.Length;
        totalButtonWidth = numButtons * buttonWidth + (numButtons - 1) * spacing;
        float startX = bounds.X + (bounds.Width - totalButtonWidth) / 2;

        for (int i = 0; i < numButtons; i++)
        {
            Rectangle btn = new Rectangle(startX + i * (buttonWidth + spacing), y, buttonWidth, buttonHeight);
            Raylib.DrawRectangleRec(btn, new Color(100, 100, 100, 255));
            Raylib.DrawText(labels[i], (int)(btn.X + 10), (int)(btn.Y + 7), 16, Color.White);

            if (Raylib.IsMouseButtonPressed(MouseButton.Left) &&
                Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), btn))
            {
                isVisible = false;

                if (i == 0) onConfirm?.Invoke();
                else onCancel?.Invoke();
            }
        }
    }

    private static Rectangle GetDialogBounds()
    {
        int screenWidth = Raylib.GetScreenWidth();
        int screenHeight = Raylib.GetScreenHeight();

        const float SMALL_PAD = 0.25f;
        int minWidth = 400;
        int minHeight = 200;

        switch (currentPlacement)
        {
            case DialogPlacement.CenterSmall:
                return new Rectangle(
                    screenWidth * SMALL_PAD,
                    screenHeight * SMALL_PAD,
                    screenWidth * (1 - SMALL_PAD * 2),
                    screenHeight * (1 - SMALL_PAD * 2));

            case DialogPlacement.CenterBig:
                return new Rectangle(0, 0, screenWidth, screenHeight);

            case DialogPlacement.LeftSmall:
                return new Rectangle(0,
                    screenHeight * SMALL_PAD,
                    screenWidth * SMALL_PAD,
                    screenHeight * (1 - SMALL_PAD * 2));

            case DialogPlacement.LeftBig:
                return new Rectangle(0, 0, screenWidth * SMALL_PAD, screenHeight);

            case DialogPlacement.RightSmall:
                return new Rectangle(
                    screenWidth * (1 - SMALL_PAD),
                    screenHeight * SMALL_PAD,
                    screenWidth * SMALL_PAD,
                    screenHeight * (1 - SMALL_PAD * 2));

            case DialogPlacement.RightBig:
                return new Rectangle(screenWidth * (1 - SMALL_PAD), 0, screenWidth * SMALL_PAD, screenHeight);

            case DialogPlacement.TopSmall:
                return new Rectangle(screenWidth * SMALL_PAD, 0,
                    screenWidth * (1 - SMALL_PAD * 2),
                    screenHeight * SMALL_PAD);

            case DialogPlacement.TopBig:
                return new Rectangle(0, 0, screenWidth, screenHeight * SMALL_PAD);

            case DialogPlacement.BottomSmall:
                return new Rectangle(screenWidth * SMALL_PAD,
                    screenHeight * (1 - SMALL_PAD),
                    screenWidth * (1 - SMALL_PAD * 2),
                    screenHeight * SMALL_PAD);

            case DialogPlacement.BottomBig:
                return new Rectangle(0,
                    screenHeight * (1 - SMALL_PAD),
                    screenWidth,
                    screenHeight * SMALL_PAD);
        }

        return new Rectangle(
            screenWidth / 2 - minWidth / 2,
            screenHeight / 2 - minHeight / 2,
            minWidth,
            minHeight);
    }
}
