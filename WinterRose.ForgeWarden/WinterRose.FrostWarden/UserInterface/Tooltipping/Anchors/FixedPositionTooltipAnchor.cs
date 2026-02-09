using Raylib_cs;

namespace WinterRose.ForgeWarden.UserInterface.Tooltipping.Anchors
{
    public sealed class FixedPositionTooltipAnchor : TooltipAnchor
    {
        public Vector2 Position { get; set; }
        public Vector2? ConstrainToSize { get; set; }

        public FixedPositionTooltipAnchor(Vector2 position)
        {
            Position = position;
        }

        public override Rectangle GetAnchorBounds()
        {
            if (ConstrainToSize.HasValue)
            {
                var s = ConstrainToSize.Value;
                return new Rectangle(Position.X, Position.Y, s.X, s.Y);
            }

            return new Rectangle(Position.X, Position.Y, 0f, 0f);
        }

        public override bool IsAnchorValid()
        {
            return true;
        }
    }
}