using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.Input;

public static class InputManager
{
    private static readonly SortedList<int, InputContext> contexts = new(Comparer<int>.Create((a, b) => b.CompareTo(a)));

    static bool prev = false;

    public static InputContext RegisterContext(InputContext context)
    {
        contexts.Add(context.Priority, context);
        return context;
    }

    public static void Update()
    {
        bool keyboardFocusGiven = false;
        bool mouseFocusGiven = false;

        InputContext highestKeyboardRequest = null;
        InputContext highestMouseRequest = null;

        foreach (var ctx in contexts.Values)
        {
            ctx.Update();

            // check for higher-priority keyboard requests
            bool hasHigherKeyboard = highestKeyboardRequest != null;
            bool hasHigherMouse = highestMouseRequest != null;

            bool higherKeyboardGivenHere = false;
            bool higherMouesGivenHere = false;


            // --- Keyboard ---
            if (ctx.IsRequestingKeyboardFocus && !hasHigherKeyboard)
            {
                ctx.HasKeyboardFocus = true;
                ctx.HasMouseFocus = true; // keyboard focus implies mouse
                highestKeyboardRequest = ctx;
                keyboardFocusGiven = true;
                higherKeyboardGivenHere = true;
                higherMouesGivenHere = true;
            }
            else
            {
                ctx.HasKeyboardFocus = false;
            }

            // --- Mouse ---
            if (ctx.IsRequestingMouseFocus && !hasHigherMouse)
            {
                ctx.HasMouseFocus = true;
                highestMouseRequest = ctx;
                mouseFocusGiven = true;
                higherMouesGivenHere = true;
            }
            else if ((!ctx.IsRequestingMouseFocus && !ctx.HasKeyboardFocus) || hasHigherMouse)
            {
                // Release mouse focus if not requesting and no keyboard focus holding it,
                // or if a higher-priority context already has mouse focus
                ctx.HasMouseFocus = false;
            }

            // Optionally, let the context know about higher-priority contexts
            if(!higherKeyboardGivenHere)
                ctx.HighestPriorityKeyboardAbove = highestKeyboardRequest;
            if(!higherMouesGivenHere)
                ctx.HighestPriorityMouseAbove = highestMouseRequest;
        }

        if(Application.Current.Window.ConfigFlags.HasFlag(Raylib_cs.ConfigFlags.TransparentWindow))
        {
            if (!keyboardFocusGiven && !mouseFocusGiven && prev)
            {
                EnablePassthrough(Windows.MyHandle.Handle);
                prev = false;
            }
            else if ((keyboardFocusGiven || mouseFocusGiven) && !prev)
            {
                DisablePassthrough(Windows.MyHandle.Handle);
                prev = true;
            }
        }
    }

    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TRANSPARENT = 0x20;
    private const int WS_EX_LAYERED = 0x80000;

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    public static void EnablePassthrough(IntPtr hwnd)
    {
        int style = GetWindowLong(hwnd, GWL_EXSTYLE);
        style |= WS_EX_LAYERED | WS_EX_TRANSPARENT;
        SetWindowLong(hwnd, GWL_EXSTYLE, style);
    }

    public static void DisablePassthrough(IntPtr hwnd)
    {
        int style = GetWindowLong(hwnd, GWL_EXSTYLE);
        style &= ~WS_EX_TRANSPARENT;
        SetWindowLong(hwnd, GWL_EXSTYLE, style);
    }
}

