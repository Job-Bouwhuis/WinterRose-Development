using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.Utility;
public static class ScissorStack
{
    private static readonly Stack<Rectangle> stack = new();

    public static void Push(Rectangle rect)
    {
        if (stack.Count == 0)
        {
            stack.Push(rect);
            BeginScissor(rect);
        }
        else
        {
            Rectangle current = stack.Peek();
            Rectangle intersection = GetIntersection(current, rect);

            stack.Push(intersection);
            BeginScissor(intersection);
        }
    }

    public static void Pop()
    {
        if (stack.Count == 0)
            return;

        stack.Pop();
        ray.EndScissorMode();

        if (stack.Count > 0)
        {
            BeginScissor(stack.Peek());
        }
    }

    public static void Clear()
    {
        while (stack.Count > 0)
            Pop();
    }

    private static void BeginScissor(Rectangle rect)
    {
        Raylib.BeginScissorMode(
            (int)rect.X,
            (int)rect.Y,
            (int)rect.Width,
            (int)rect.Height
        );
    }

    private static Rectangle GetIntersection(Rectangle a, Rectangle b)
    {
        float x1 = System.Math.Max(a.X, b.X);
        float y1 = System.Math.Max(a.Y, b.Y);
        float x2 = System.Math.Min(a.X + a.Width, b.X + b.Width);
        float y2 = System.Math.Min(a.Y + a.Height, b.Y + b.Height);

        if (x2 < x1 || y2 < y1)
            return new Rectangle(0, 0, 0, 0); // no overlap

        return new Rectangle(x1, y1, x2 - x1, y2 - y1);
    }
}