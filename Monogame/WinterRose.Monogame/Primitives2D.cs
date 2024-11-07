using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Linq;
using WinterRose.Monogame.Worlds;

namespace WinterRose.Monogame
{
    /// <summary>
    /// A helper class to draw simple 2D shapes
    /// </summary>
    public static class Primitives2D
    {
        private static Texture2D _pixel; // A 1x1 white pixel texture
        private static ConcurrentStack<Primitive2DDrawRequest> drawRequests = new();

        static Primitives2D()
        {
            _pixel = MonoUtils.CreateTexture(1, 1, Color.White);
        }

        /// <summary>
        /// Draws a line from start to end with the given color and thickness
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="color"></param>
        /// <param name="thickness"></param>
        public static void DrawLine(Vector2 start, Vector2 end, Color color, int thickness = 1)
        {
            drawRequests.Push(new((SpriteBatch batch, Vector2 s, Vector2 e, Color c, int t) =>
            {
                Vector2 edge = e - s;
                float angle = (float)Math.Atan2(edge.Y, edge.X);

                batch.Draw(_pixel, s, null, c, angle, Vector2.Zero, new Vector2(edge.Length(), t), SpriteEffects.None, 0);
            }, start, end, color, thickness));
            Universe.RequestRender = true;
        }
        /// <summary>
        /// Draws a square at the given position with the given size and color
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        /// <param name="color"></param>
        public static void DrawSquare(Vector2 pos, Vector2 size, Color color)
        {
            drawRequests.Push(new((SpriteBatch batch, Vector2 p, Vector2 s, Color c) =>
            {
                batch.Draw(_pixel, pos, null, color, 0, new(0.5f, 0.5f), size, SpriteEffects.None, 0.5f);
            }, pos, size, color));
            Universe.RequestRender = true;
        }
        internal static void CommitDraw(SpriteBatch batch)
        {
            while(drawRequests.TryPop(out var request))
                request.Func.DynamicInvoke(request.Args.Prepend(batch).ToArray());
        }
        private record Primitive2DDrawRequest(Delegate Func, params object[] Args);
    }
}
