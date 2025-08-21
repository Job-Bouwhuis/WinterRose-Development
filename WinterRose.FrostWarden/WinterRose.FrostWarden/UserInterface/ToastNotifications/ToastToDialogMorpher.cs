using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.Tweens;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes;
using WinterRose.ForgeWarden.UserInterface.DialogBoxes.Boxes;

namespace WinterRose.ForgeWarden.UserInterface.ToastNotifications;
internal static class ToastToDialogMorpher
{
    private class MorphContext
    {
        public Toast Toast;
        public Dialog Dialog;
        public float MorphDuration;
        public float BeginToastAlphaFadeoutFraction = 0.05f;
        public float ToastAlphaFadeEndFraction = 0.4f;
        public float BeginDialogContentFadeinFraction = 0.25f;
        public float Progress = 0f;

        public Rectangle StartPosition;
        public float shadowTopStart;
        public float shadowBottomStart;
        public float shadowLeftStart;
        public float shadowRightStart;

        public float ToastFadeElapsed;
        public float DialogFadeElapsed;
    }

    private static readonly List<MorphContext> activeMorphs = new();

    public static bool TryStartMorph(Toast toast, Dialog dialog,
                                     float morphDuration = 0.5f,
                                     float toastFadeStart = 0.05f,
                                     float toastFadeEnd = 0.4f,
                                     float dialogFadeStart = 0.55f)
    {
        if (!Dialogs.IsDialogSpotAvailible(dialog.Placement))
        {
            toast.Close();
            Dialogs.Show(dialog);
            return false;
        }

        Toasts.RemoveImmediately(toast, toast.Region);
        toast.IsMorphDrawing = true;
        dialog.DrawContentOnly = true;
        Dialogs.AddImmediately(dialog);
        dialog.Style.ContentAlpha = 0;
        dialog.CurrentAnim = new()
        {
            Completed = true,
            Elapsed = 1,
            ScaleHeight = 1,
            ScaleWidth = 1
        };

        activeMorphs.Add(new MorphContext
        {
            Toast = toast,
            Dialog = dialog,
            MorphDuration = morphDuration,
            BeginToastAlphaFadeoutFraction = toastFadeStart,
            ToastAlphaFadeEndFraction = toastFadeEnd,
            BeginDialogContentFadeinFraction = dialogFadeStart,
            Progress = 0f,
            StartPosition = toast.CurrentPosition,
            shadowTopStart = toast.Style.ShadowSizeTop,
            shadowBottomStart = toast.Style.ShadowSizeBottom,
            shadowLeftStart = toast.Style.ShadowSizeLeft,
            shadowRightStart = toast.Style.ShadowSizeRight,
        });

        return true;
    }

    public static void Update()
    {
        for (int i = activeMorphs.Count - 1; i >= 0; i--)
        {
            var ctx = activeMorphs[i];

            // Progress in seconds
            ctx.Progress += Time.deltaTime / ctx.MorphDuration;
            float rawProgress = Math.Clamp(ctx.Progress, 0f, 1f);

            // Evaluate morph curve
            float t = Curves.EaseOutBackLow.Evaluate(rawProgress);

            Rectangle dialogBounds = Dialogs.GetDialogBounds(ctx.Dialog.Placement);

            // Morph position & size with overshoot
            ctx.Toast.CurrentPosition.X = Lerp(ctx.StartPosition.X, dialogBounds.X, t);
            ctx.Toast.CurrentPosition.Y = Lerp(ctx.StartPosition.Y, dialogBounds.Y, t);
            ctx.Toast.CurrentPosition.Width = Lerp(ctx.StartPosition.Width, dialogBounds.Width, t);
            ctx.Toast.CurrentPosition.Height = Lerp(ctx.StartPosition.Height, dialogBounds.Height, t);

            ctx.Toast.Style.ShadowSizeLeft = Lerp(ctx.shadowLeftStart, ctx.Dialog.Style.ShadowSizeLeft, t);
            ctx.Toast.Style.ShadowSizeTop = Lerp(ctx.shadowTopStart, ctx.Dialog.Style.ShadowSizeTop, t);
            ctx.Toast.Style.ShadowSizeRight = Lerp(ctx.shadowRightStart, ctx.Dialog.Style.ShadowSizeRight, t);
            ctx.Toast.Style.ShadowSizeBottom = Lerp(ctx.shadowBottomStart, ctx.Dialog.Style.ShadowSizeBottom, t);

            // Style interpolation (background, border, shadow)
            ctx.Toast.Style.Background = LerpColor(ctx.Toast.Style.Background, ctx.Dialog.Style.Background, t);
            ctx.Toast.Style.Border = LerpColor(ctx.Toast.Style.Border, ctx.Dialog.Style.Border, t);
            ctx.Toast.Style.Shadow = LerpColor(ctx.Toast.Style.Shadow, ctx.Dialog.Style.Shadow, t);

            // Fade calculations using *actual t*, mapped to fade interval
            // Toast content fadeout
            if (rawProgress >= ctx.BeginToastAlphaFadeoutFraction)
            {
                float toastAlphaT = (rawProgress - ctx.BeginToastAlphaFadeoutFraction) /
                                    (ctx.ToastAlphaFadeEndFraction - ctx.BeginToastAlphaFadeoutFraction);
                toastAlphaT = Math.Clamp(toastAlphaT, 0f, 1f);
                ctx.Toast.Style.ContentAlpha = 1f - toastAlphaT;
            }

            // Dialog content fadein
            if (rawProgress >= ctx.BeginDialogContentFadeinFraction)
            {
                float dialogAlphaT = (rawProgress - ctx.BeginDialogContentFadeinFraction) /
                                     (1f - ctx.BeginDialogContentFadeinFraction);
                dialogAlphaT = Math.Clamp(dialogAlphaT, 0f, 1f);
                ctx.Dialog.Style.ContentAlpha = dialogAlphaT;
                ctx.Dialog.CurrentAnim = ctx.Dialog.CurrentAnim with
                {
                    Alpha = dialogAlphaT,
                };
            }

            // Morph complete
            if (rawProgress >= 1f)
            {
                ctx.Dialog.DrawContentOnly = false;
                activeMorphs.RemoveAt(i);
            }
        }
    }


    public static void Draw()
    {
        for (int i = 0; i < activeMorphs.Count; i++)
        {
            MorphContext? ctx = activeMorphs[i];
            DrawMorph(ctx);
        }
    }

    private static void DrawMorph(MorphContext ctx)
    {
        Rectangle toastRect = ctx.Toast.CurrentPosition;

        // Shadow
        Rectangle shadow = new Rectangle(
            toastRect.X - ctx.Toast.Style.ShadowSizeLeft,
            toastRect.Y - ctx.Toast.Style.ShadowSizeTop,
            toastRect.Width + ctx.Toast.Style.ShadowSizeLeft + ctx.Toast.Style.ShadowSizeRight,
            toastRect.Height + ctx.Toast.Style.ShadowSizeTop + ctx.Toast.Style.ShadowSizeBottom);
        ray.DrawRectangleRec(shadow,
            ctx.Toast.Style.Shadow.WithAlpha(ctx.Dialog.Style.ShadowRaw.A));

        // Background
        Color back = ctx.Toast.Style.Background.WithAlpha(ctx.Dialog.Style.BackgroundRaw.A);
        ray.DrawRectangleRec(toastRect, back);

        // Border
        Color bord = ctx.Toast.Style.Border.WithAlpha(ctx.Dialog.Style.BorderRaw.A);
        ray.DrawRectangleLinesEx(toastRect, 2, bord);

        // Content area with padding
        Rectangle contentArea = new Rectangle(
            toastRect.X + UIConstants.CONTENT_PADDING,
            toastRect.Y + UIConstants.CONTENT_PADDING,
            toastRect.Width - UIConstants.CONTENT_PADDING * 2,
            toastRect.Height - UIConstants.CONTENT_PADDING * 2
            );


        // Draw toast content only if alpha > 0
        if (ctx.Toast.Style.ContentAlpha > 0f)
            ctx.Toast.DrawContent(contentArea);

        // Draw dialog content only if alpha > 0
        if (ctx.Dialog.Style.ContentAlpha > 0f)
            ctx.Dialog.DrawContent(contentArea);
    }

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;

    private static Color LerpColor(Color a, Color b, float t)
    {
        return new Color(
            (byte)Lerp(a.R, b.R, t),
            (byte)Lerp(a.G, b.G, t),
            (byte)Lerp(a.B, b.B, t),
            (byte)Lerp(a.A, b.A, t)
        );
    }
}

