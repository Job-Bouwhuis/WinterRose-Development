namespace WinterRose.ForgeWarden.Geometry
{
    internal static class MathUtil
    {
        public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
        {
            return a + (b - a) * t;
        }
    }
}
