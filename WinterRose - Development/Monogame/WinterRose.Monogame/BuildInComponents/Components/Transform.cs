using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WinterRose.Monogame.Attributes;
using WinterRose.Monogame.Interfaces;
using WinterRose.Monogame.Worlds;

namespace WinterRose.Monogame
{
    /// <summary>
    /// The transform of a <see cref="WorldObject"/>
    /// </summary>
    [ComponentLimit]
    public class Transform : ObjectComponent
    {
        private Transform() { }
        internal Transform(WorldObject owner) { _owner = owner; }

        /// <summary>
        /// The objects position
        /// </summary>
        public Vector2 position
        {
            get
            {
                return _position;
            }
            set
            {
                if (value == _position) return;

                var delta = value - _position; // Calculate the change in position

                IPositionChangedCallback[] callbacks = owner.FetchComponents<IPositionChangedCallback>();
                SquareCollider? col = FetchComponent<SquareCollider>();
                if (col is not null && col.Enabled)
                {
                    foreach (var collider in col.pastColliders)
                    {
                        if (collider.other == col)
                            continue;

                        // Check for collisions on different sides
                        if (collider.CollisionSide.HasFlag(CollisionSide.Left))
                        {
                            if (delta.X < 0)
                            {
                                delta.X = 0.2f;
                            }
                        }

                        if (collider.CollisionSide.HasFlag(CollisionSide.Right))
                        {
                            if (delta.X > 0)
                            {
                                delta.X = -0.2f;
                            }
                        }

                        if (collider.CollisionSide.HasFlag(CollisionSide.Top))
                        {
                            if (delta.Y < 0)
                            {
                                delta.Y = 0;
                            }
                        }

                        if (collider.CollisionSide.HasFlag(CollisionSide.Bottom))
                        {
                            if (delta.Y > 0)
                            {
                                delta.Y = 0;
                            }
                        }
                    }
                }

                callbacks?.Foreach(x => x.OnBeforePositionChanged(_position + delta));
                _position += delta;
                callbacks?.Foreach(x => x.OnAfterPositionChanged());
                Universe.RequestRender = true;
                UpdateChildPositions(delta);
            }
        }
        /// <summary>
        /// The position relative to the <see cref="parent"/>
        /// </summary>
        public Vector2 localPosition
        {
            get
            {
                if (parent is null)
                    return position;
                return position - parent.transform.position;
            }
            set
            {

            }
        }
        /// <summary>
        /// The objects rotation in degrees
        /// </summary>
        public float rotation
        {
            get
            {
                return _rotation;
            }
            set
            {

                float deltaRotation = value - rotation;
                _rotation = value;
                UpdateChildRotations(deltaRotation);
                Universe.RequestRender = true;
            }
        }
        public float rotationRad
        {
            get { return MathS.ToRadians(rotation); }
            set { rotation = MathS.ToDegrees(value); }
        }
        /// <summary>
        /// The objects scale
        /// </summary>
        public Vector2 scale
        {
            get
            {
                return _scale;
            }
            set
            {
                Universe.RequestRender = true;
                _scale = value;
            }
        }
        /// <summary>
        /// The objects forward direction
        /// </summary>
        public Vector2 up
        {
            get
            {
                float radians = MathHelper.ToRadians(rotation - 90);
                return new Vector2(-MathF.Sin(radians), MathF.Cos(radians));
            }
        }

        private Transform? _parent;
        public Transform? parent
        {
            get => _parent;
            set
            {
                if (value is null && _parent is not null)
                {
                    _parent.children.Remove(this);
                }
                _parent = value;
                _parent?.children.Add(this);
            }
        }

        /// <summary>
        /// Whether this transform has any chilren. Enumerate them by foreaching over a transform
        /// </summary>
        public bool HasChildren => children.Count is not 0;

        private List<Transform> children = new();

        private Vector2 _position;
        private float _rotation;
        private Vector2 _scale = new(1, 1);

        /// <summary>
        /// Gets the world position from the given screen position relative to the camera
        /// </summary>
        /// <param name="cam"></param>
        /// <returns>The position of <paramref name="pos"/> in the world based on the priveded <see cref="Camera"/> <paramref name="cam"/> position, 
        /// <br></br> if <paramref name="cam"/> is null, returns <paramref name="pos"/></returns>
        public static Vector2 ScreenToWorldPos(Vector2 pos, Camera? cam)
        {
            if (cam is null)
                return pos;

            Vector2 camPos = cam.transform.position;
            Vector2 camBoundsHalf = cam.Bounds / 2;
            Vector2 windowResolutionHalf = MonoUtils.WindowResolution / 2;

            // Translate the screen position to world position considering the camera position
            Vector2 result = pos + camPos - camBoundsHalf - windowResolutionHalf;
            return result + (MonoUtils.WindowResolution / 2);
        }
        /// <summary>
        /// Rotates the object so that its forward direction faces the given <paramref name="pos"/>
        /// </summary>
        /// <param name="pos"></param>
        public void LookAt(Vector2 pos)
        {
            var direction = Vector2.Normalize(pos - position);

            var angle = MathHelper.ToDegrees(MathF.Atan2(direction.Y, direction.X));

            transform.rotation = angle;
        }

        private void UpdateChildPositions(Vector2 delta)
        {
            foreach (var child in children)
            {
                child.position += delta;
            }
        }
        private void UpdateChildRotations(float deltaRotation)
        {
            foreach (var child in children)
            {
                Vector2 relativePosition = child.position - position;
                Vector2 rotatedPosition = RotateVector(relativePosition, MathS.ToRadians(deltaRotation));
                child.position = position + rotatedPosition;
                child.rotation += deltaRotation;
            }
        }
        private Vector2 RotateVector(Vector2 vector, float angleInRadians)
        {
            float cos = MathF.Cos(angleInRadians);
            float sin = MathF.Sin(angleInRadians);
            float x = vector.X * cos - vector.Y * sin;
            float y = vector.X * sin + vector.Y * cos;
            return new Vector2(x, y);
        }

        public IEnumerator<Transform> GetEnumerator()
        {
            return children.GetEnumerator();
        }
    }
}