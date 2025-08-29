using Raylib_cs;
using System.Formats.Tar;
using WinterRose.ForgeWarden.TextRendering;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes.Enums;

namespace WinterRose.ForgeWarden.UserInterface.DialogBoxes.Boxes;

//public class ProgressDialog : Dialog
//{
//    public float ProgressValue { get; private set; }

//    /// <summary>When true, show an indeterminate sweeping bar.</summary>
//    public bool UnknownProgress { get; set; }

//    private float cycleDuration = 2;

//    enum IndetPhase
//    {
//        GrowingLeft,
//        MovingRight,
//        ShrinkingRight,
//        GrowingRight,
//        MovingLeft,
//        ShrinkingLeft
//    }

//    IndetPhase phase = IndetPhase.GrowingLeft;

//    float segWidth = 0f;
//    float segLeft;

//    int padding;
//    int lastPadding;
//    // Desired total duration for the full cycle (seconds)


//    private int Padding
//    {
//        get => padding;
//        set
//        {
//            if (value == lastPadding)
//                return;
//            lastPadding = padding;
//            padding = value;
//            segLeft = Dialogs.GetDialogBounds(Placement).X + padding;
//        }
//    }

//    private Rectangle barBg;
//    private float maxWidth;
//    private float IndetMaxWidthFrac = 0.3f;
//    private float growSpeed;
//    private float moveSpeed;
//    private float shrinkSpeed;

//    public ProgressDialog(
//        string title,
//        string message,
//        float initialProgress,
//        DialogPlacement placement = DialogPlacement.CenterSmall,
//        DialogPriority priority = DialogPriority.Normal,
//        string[]? buttons = null,
//        Func<bool>[]? onButtonClick = null,
//        Action<UIContext>? onImGui = null)
//        : base(title, message, placement, priority, buttons ?? [], onButtonClick ?? [], onImGui)
//    {
//        ProgressValue = Math.Clamp(initialProgress, 0f, 1f);
//        UnknownProgress = false;
//    }

//    public void SetProgress(float value)
//    {
//        ProgressValue = Math.Clamp(value, 0f, 1f);
//        UnknownProgress = false;
//    }

//    public override void Update()
//    {
//    }

//    public override void DrawContent(Rectangle bounds, float contentAlpha, ref int padding, ref float innerWidth, ref int y)
//    {
//        Padding = padding;
//        barBg = new Rectangle(bounds.X + padding, y, innerWidth, 20);

//        maxWidth = barBg.Width * IndetMaxWidthFrac;

//        float phaseDuration = cycleDuration / 6f;
//        growSpeed = moveSpeed = (barBg.Width - maxWidth) / phaseDuration;
//        shrinkSpeed = maxWidth / phaseDuration;

//        // Colors with content alpha applied
//        Color bg = Style.ProgressBarBackground;
//        Color fill = Style.ProgressBarFill;
//        Color textCol = Style.BarText;

//        ray.DrawRectangleRec(barBg, bg);
//        if (UnknownProgress)
//        {
//            switch (phase)
//            {
//                case IndetPhase.GrowingLeft:
//                    segWidth += growSpeed * Time.deltaTime;
//                    segLeft = barBg.X; // left anchored
//                    if (segWidth >= maxWidth)
//                    {
//                        segWidth = maxWidth;
//                        phase = IndetPhase.MovingRight;
//                    }
//                    break;

//                case IndetPhase.MovingRight:
//                    segLeft += moveSpeed * Time.deltaTime;
//                    if (segLeft + segWidth >= barBg.X + barBg.Width)
//                    {
//                        segLeft = barBg.X + barBg.Width - segWidth;
//                        phase = IndetPhase.ShrinkingRight;
//                    }
//                    break;

//                case IndetPhase.ShrinkingRight:
//                    segWidth -= growSpeed * Time.deltaTime;
//                    segLeft = barBg.X + barBg.Width - segWidth; // right anchored
//                    if (segWidth <= 0f)
//                    {
//                        segWidth = 0f;
//                        phase = IndetPhase.GrowingRight;
//                    }
//                    break;

//                case IndetPhase.GrowingRight:
//                    segWidth += growSpeed * Time.deltaTime;
//                    segLeft = barBg.X + barBg.Width - segWidth; // right anchored
//                    if (segWidth >= maxWidth)
//                    {
//                        segWidth = maxWidth;
//                        phase = IndetPhase.MovingLeft;
//                    }
//                    break;

//                case IndetPhase.MovingLeft:
//                    segLeft -= moveSpeed * Time.deltaTime;
//                    if (segLeft <= barBg.X)
//                    {
//                        segLeft = barBg.X;
//                        phase = IndetPhase.ShrinkingLeft;
//                    }
//                    break;

//                case IndetPhase.ShrinkingLeft:
//                    segWidth -= growSpeed * Time.deltaTime;
//                    segLeft = barBg.X; // left anchored
//                    if (segWidth <= 0f)
//                    {
//                        segWidth = 0f;
//                        phase = IndetPhase.GrowingLeft;
//                    }
//                    break;
//            }

//            Rectangle seg = new Rectangle(segLeft, barBg.Y, segWidth, barBg.Height);
//            ray.DrawRectangleRec(seg, fill);
//        }
//        else
//        {
//            float clamped = Math.Clamp(ProgressValue, 0f, 1f);
//            Rectangle barFillRect = new Rectangle(barBg.X, barBg.Y, barBg.Width * clamped, barBg.Height);
//            ray.DrawRectangleRec(barFillRect, fill);
//        }

//        // Label
//        string progressText = UnknownProgress
//            ? "Working..."
//            : $"{MathF.Round(ProgressValue * 100f, 1)}%";

//        // Scale font a touch with bar height, but keep it readable
//        int fontSize = Math.Clamp((int)(barBg.Height * 0.7f), 12, 20);
//        int textWidth = ray.MeasureText(progressText, fontSize);
//        int textX = (int)(barBg.X + (barBg.Width - textWidth) / 2);
//        int textY = (int)(barBg.Y + (barBg.Height - fontSize) / 2);

//        ray.DrawText(progressText, textX, textY, fontSize, textCol);

//        y += 30; // advance layout
//    }

//    private static Color WithAlpha(Color orig, float opacity)
//    {
//        int a = (int)(orig.A * Math.Clamp(opacity, 0f, 1f));
//        return new Color(orig.R, orig.G, orig.B, a);
//    }
//}