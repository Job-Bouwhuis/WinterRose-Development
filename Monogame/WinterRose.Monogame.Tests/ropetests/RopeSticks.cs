using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.Tests.ropetests
{
    public class Point
    {
        public Vector2 position, previousPosition;
        public bool locked;
    }

    public class Stick
    {
        public Point pointA, pointB;
        public float length;
    }

    public class RopeSim : ObjectBehavior
    {
        public List<Point> points = new();
        public List<Stick> sticks = new();

        public float gravity = 0.5f;

        protected override void Start()
        {
            //Hirarchy.AllowedForFieldTree.Add(typeof(Stick));
            //Hirarchy.AllowedForFieldTree.Add(typeof(Point));

            owner.AddDrawBehavior((owner, batch) => Draw(batch));

            // generate points and sticks

            for (int i = 0; i < 10; i++)
            {
                Point point = new Point();
                point.position = new Vector2(100 + i * 20, 100);
                point.previousPosition = point.position;
                points.Add(point);

                if (i > 0)
                {
                    Stick stick = new Stick();
                    stick.pointA = points[i - 1];
                    stick.pointB = point;
                    stick.length = 20;
                    sticks.Add(stick);
                }
            }

            points[0].locked = true;
        }

        protected override void Update()
        {
            foreach (var point in points)
            {
                if (!point.locked)
                {
                    Vector2 beforePosChange = point.position;
                    point.position += point.position - point.previousPosition;
                    point.position += Vector2.UnitY * gravity * Time.SinceLastFrame * Time.SinceLastFrame;
                    point.previousPosition = beforePosChange;
                }
            }

            foreach(int i in 10)
            {
                foreach (Stick stick in sticks)
                {
                    Vector2 stickCenter = (stick.pointA.position + stick.pointB.position) / 2;
                    Vector2 stickDirection = (stick.pointB.position - stick.pointA.position).Normalized();

                    if(!stick.pointA.locked)
                    {
                        stick.pointA.position = stickCenter - stickDirection * stick.length / 2;
                    }

                    if (!stick.pointB.locked)
                    {
                        stick.pointB.position = stickCenter + stickDirection * stick.length / 2;
                    }
                }
            }
        }


        private void Draw(SpriteBatch batch)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                batch.DrawLine(points[i].position, points[i + 1].position, Color.White);
            }

            foreach (var point in points)
            {
                batch.DrawCircle(point.position, 5, Color.White);
            }
        }
    }
}
