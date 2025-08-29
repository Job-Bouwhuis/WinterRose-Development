using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.Tweens;
public static class Tweener
{
    public static T Tween<T>(T start, T end, float t, params Curve[] curves)
    {
        if (typeof(T) == typeof(float))
        {
            Curve? curve = curves.Length > 0 ? curves[0] : null;
            float s = Convert.ToSingle(start);
            float e = Convert.ToSingle(end);
            float val = curve != null ? LerpWithCurve(s, e, t, curve) : Lerp(s, e, t);
            return (T)Convert.ChangeType(val, typeof(T));
        }

        if (typeof(T) == typeof(Vector2))
        {
            Vector2 s = (Vector2)(object)start;
            Vector2 e = (Vector2)(object)end;

            Curve? curveX = curves.Length > 0 ? curves[0] : throw new NullReferenceException("Did not provide a curve"); ;
            Curve? curveY = curves.Length > 1 ? curves[1] : curveX;

            return (T)(object)new Vector2(
                LerpWithCurve(s.X, e.X, t, curveX),
                LerpWithCurve(s.Y, e.Y, t, curveY)
            );
        }

        if (typeof(T) == typeof(Vector3))
        {
            Vector3 s = (Vector3)(object)start;
            Vector3 e = (Vector3)(object)end;

            Curve? curveX = curves.Length > 0 ? curves[0] : throw new NullReferenceException("Did not provide a curve");
            Curve? curveY = curves.Length > 1 ? curves[1] : curveX;
            Curve? curveZ = curves.Length > 2 ? curves[2] : curveX;

            return (T)(object)new Vector3(
                LerpWithCurve(s.X, e.X, t, curveX),
                LerpWithCurve(s.Y, e.Y, t, curveY),
                LerpWithCurve(s.Y, e.Y, t, curveZ)
            );
        }

        if (typeof(T) == typeof(Rectangle))
        {
            Rectangle s = (Rectangle)(object)start;
            Rectangle e = (Rectangle)(object)end;

            Curve? curveX = curves.Length > 0 ? curves[0] : throw new NullReferenceException("Did not provide a curve");
            Curve? curveY = curves.Length > 1 ? curves[1] : curveX;
            Curve? curveW = curves.Length > 2 ? curves[2] : curveX;
            Curve? curveH = curves.Length > 3 ? curves[3] : curveX;

            return (T)(object)new Rectangle(
                LerpWithCurve(s.X, e.X, t, curveX),
                LerpWithCurve(s.Y, e.Y, t, curveY),
                LerpWithCurve(s.Width, e.Width, t, curveW),
                LerpWithCurve(s.Height, e.Height, t, curveH)
            );
        }

        throw new NotSupportedException($"Tweener.Tween does not support type {typeof(T)}");
    }

    private static float Lerp(float start, float end, float t)
    {
        return start + (end - start) * t;
    }

    private static float LerpWithCurve(float start, float end, float t, Curve? curve)
    {
        if (curve != null) t = curve.Evaluate(t);
        return Lerp(start, end, t);
    }
}

