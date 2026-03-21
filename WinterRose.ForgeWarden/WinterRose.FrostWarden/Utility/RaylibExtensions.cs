using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;

namespace WinterRose.ForgeWarden;

public static class RaylibExtensions
{
    extension(Color c)
    {
        [get: MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string HexA => $"#{c.R:X2}{c.G:X2}{c.B:X2}{c.A:X2}";
        public string Hex => $"#{c.R:X2}{c.G:X2}{c.B:X2}";
    }

    extension(Sound s)
    {
        /// <summary>
        /// Set the volume of a sound
        /// </summary>
        /// <param name="volume">A value between 0 and 1 representing 0% to 100% volume</param>
        public void SetVolume(float volume) => ray.SetSoundVolume(s, volume);

        /// <summary>
        /// Set the direction of the audio where -1 is left, and 1 is right
        /// </summary>
        /// <param name="panning"></param>
        public void SetPanning(float panning) => ray.SetSoundPan(s, panning);

        public void SetSoundDirection(
            Vector2 soundPos,
            Vector2 listenerPos,
            float fullHearingRange,
            float volume100Range,
            float volume0Range)
        {
            float dx = soundPos.X - listenerPos.X;
            float distance = MathF.Abs(dx);

            float pan = dx / fullHearingRange;
            pan = Math.Clamp(pan, -1.0f, 1.0f);

            float volume;

            if (distance <= volume100Range)
            {
                volume = 1.0f;
            }
            else if (distance >= volume0Range)
            {
                volume = 0.0f;
            }
            else
            {
                float t = (distance - volume100Range) / (volume0Range - volume100Range);
                volume = 1.0f - t;
            }

            s.SetPanning(pan);
            s.SetVolume(volume);
        }
    }


    extension(Rectangle r)
    {
        public float Left => r.X;
        public float Right => r.X + r.Width;
        public float Top => r.Y;
        public float Bottom => r.Y + r.Height;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(Rectangle other)
        {
            return other.Left >= r.Left &&
                   other.Right <= r.Right &&
                   other.Top >= r.Top &&
                   other.Bottom <= r.Bottom;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(Vector2 point)
        {
            return Raylib.CheckCollisionPointRec(point, r);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rectangle Offset(int x, int y)
        {
            return new Rectangle(r.X + x, r.Y + y, r.Width, r.Height);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rectangle Inflate(int x, int y)
        {
            return new Rectangle(
                r.X - x,
                r.Y - y,
                r.Width + x * 2,
                r.Height + y * 2
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Intersects(Rectangle other)
        {
            return Raylib.CheckCollisionRecs(r, other);
        }
    }
}
