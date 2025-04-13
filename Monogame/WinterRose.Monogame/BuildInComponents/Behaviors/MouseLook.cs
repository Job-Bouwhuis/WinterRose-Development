using Microsoft.Xna.Framework;
using System;
using WinterRose.StaticValueModifiers;

namespace WinterRose.Monogame
{
    /// <summary>
    /// Makes the object rotate towards the mouse location
    /// </summary>
    public class MouseLook : ObjectBehavior
    {
        /// <summary>
        /// The speed at which it rotates
        /// </summary>
        public StaticAdditiveModifier<float> RotateSpeedModifier { get; private set; } = new() { BaseValue = 1 };

        /// <summary>
        /// Whether the rotating should snap, and not go smoothly
        /// </summary>
        public bool Snap { get; set; } = false;

        protected override void Update()
        {
            Vector2 mousePos = Input.MousePosition;
            mousePos = Transform.ScreenToWorldPos(mousePos, Camera.current);
            transform.LookAt(mousePos);
            return;

            // need to fix rotation snapping from 179 degrees to -180
            //var direction = Vector2.Normalize(mousePos - transform.position);

            //var angle = MathHelper.ToDegrees(MathF.Atan2(direction.Y, direction.X));

            //transform.rotation = 
            //    float.Lerp(transform.rotation, 
            //    angle, 
            //    Time.deltaTime * RotateSpeedModifier);
        }
    }
}
