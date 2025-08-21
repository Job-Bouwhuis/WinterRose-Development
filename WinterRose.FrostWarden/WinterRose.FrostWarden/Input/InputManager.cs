using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.Input;

public static class InputManager
{
    private static readonly SortedList<int, InputContext> contexts = new(Comparer<int>.Create((a, b) => b.CompareTo(a)));

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
    }

}

