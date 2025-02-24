using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame;

public class Polygon : Renderer
{
    public List<PolygonPoint> Points { get; set; } = [];
    /// <summary>
    /// Values between 0 and 1 meaning percentage of the width and height of the polygon bounds
    /// </summary>
    public Vector2 Origin { get; set; } = new(.5f, .5f);

    public Polygon(params PolygonPoint[] points)
    {
        Points.AddRange(points);
    }

    public Polygon Triangle => new(new PolygonPoint(new(0, 0)), new PolygonPoint(new(1, 0)), new PolygonPoint(new(.5f, 1)));

    public override RectangleF Bounds
    {
        get
        {
            Vector2 minXY = Points.Aggregate((min, next) => new Vector2(Math.Min(min.X, next.X), Math.Min(min.Y, next.Y)));
            
            float width = Points.Max(p => p.X) - minXY.X;
            float height = Points.Max(p => p.Y) - minXY.Y;

            return new RectangleF(width, height, minXY.X, minXY.Y);
        }
    }

    public override TimeSpan DrawTime { get; protected set; }

    private Vector2 GetBounds()
    {
        float minX = Points.Min(p => p.X);
        float minY = Points.Min(p => p.Y);
        float maxX = Points.Max(p => p.X);
        float maxY = Points.Max(p => p.Y);
        return new Vector2(maxX - minX, maxY - minY);
    }

    public void Scale(float scale)
    {
        Vector2 bounds = GetBounds();
        Vector2 origin = bounds * Origin;
        for (int i = 0; i < Points.Count; i++)
        {
            Points[i] = (Points[i] - origin) * scale + origin;
        }
    }

    public void Translate(Vector2 translation)
    {
        for (int i = 0; i < Points.Count; i++)
        {
            Points[i] += translation;
        }
    }

    public void SetPosition(Vector2 position)
    {
        Vector2 bounds = GetBounds();
        Vector2 origin = bounds * Origin;
        Vector2 translation = position - origin;
        Translate(translation);
    }

    public override void Render(SpriteBatch batch)
    {
        foreach (var point in Points)
        {

        }
    }

    public struct PolygonPoint
    {
        public Vector2 Point { get; set; }
        public Color Color { get; set; }

        public readonly float X => Point.X;
        public readonly float Y => Point.Y;

        public PolygonPoint(Vector2 point, Color color)
        {
            Point = point;
            Color = color;
        }

        public PolygonPoint(Vector2 point) : this(point, Color.White) { }

        public static implicit operator PolygonPoint(Vector2 point) => new(point);
        public static implicit operator Vector2(PolygonPoint point) => point.Point;

    }
}
