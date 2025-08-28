using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.Input;

public static class InputManager
{
    private static readonly SortedList<int, List<InputContext>> contexts = new(Comparer<int>.Create((a, b) => b.CompareTo(a)));

    private static readonly Dictionary<InputContext, int> contextPriorities = new();

    internal static bool EnablePassThrough { get; private set; } = false;

    public static InputContext RegisterContext(InputContext context)
    {
        if (!contexts.TryGetValue(context.Priority, out var list))
        {
            list = new List<InputContext>();
            contexts.Add(context.Priority, list);
        }

        list.Add(context);
        contextPriorities[context] = context.Priority;
        return context;
    }

    public static void Update()
    {
        // --- Detect and apply priority edits (cheap: only touches changed contexts) ---
        if (contextPriorities.Count > 0)
        {
            // collect changed contexts (small allocation only when there are changes)
            List<InputContext> changed = null;
            foreach (var kvp in contextPriorities)
            {
                var ctx = kvp.Key;
                int oldPriority = kvp.Value;
                int newPriority = ctx.Priority;
                if (oldPriority != newPriority)
                {
                    changed ??= new List<InputContext>();
                    changed.Add(ctx);
                }
            }

            if (changed != null)
            {
                foreach (var ctx in changed)
                {
                    var old = contextPriorities[ctx];

                    // remove from old bucket
                    if (contexts.TryGetValue(old, out var oldList))
                    {
                        oldList.Remove(ctx);
                        if (oldList.Count == 0)
                            contexts.Remove(old);
                    }

                    // add to new bucket
                    if (!contexts.TryGetValue(ctx.Priority, out var newList))
                    {
                        newList = new List<InputContext>();
                        contexts.Add(ctx.Priority, newList);
                    }

                    newList.Add(ctx);
                    contextPriorities[ctx] = ctx.Priority;
                }
            }
        }

        bool keyboardFocusGiven = false;
        bool mouseFocusGiven = false;

        InputContext highestKeyboardRequest = null;
        InputContext highestMouseRequest = null;

        // iterate buckets in sorted order (highest priority buckets first),
        // and then each context in insertion order inside the bucket.
        foreach (var bucket in contexts.Values)
        {
            foreach (var ctx in bucket)
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
                if (!higherKeyboardGivenHere)
                    ctx.HighestPriorityKeyboardAbove = highestKeyboardRequest;
                if (!higherMouesGivenHere)
                    ctx.HighestPriorityMouseAbove = highestMouseRequest;
            }
        }

        if (Application.Current.Window.ConfigFlags.HasFlag(Raylib_cs.ConfigFlags.TransparentWindow))
        {
            if (!keyboardFocusGiven && !mouseFocusGiven && EnablePassThrough)
            {
                EnablePassthrough(Windows.MyHandle.Handle);
                EnablePassThrough = false;
            }
            else if ((keyboardFocusGiven || mouseFocusGiven) && !EnablePassThrough)
            {
                DisablePassthrough(Windows.MyHandle.Handle);
                EnablePassThrough = true;
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

