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
        public StaticAdditiveModifier<float> RotateSpeedModifier { get; private set; } = new() { BaseValue = 20 };

        /// <summary>
        /// Whether the rotating should snap, and not go smoothly
        /// </summary>
        public bool Snap { get; set; } = false;

        private void Update()
        {
            Vector2 mousePos = Input.MousePosition;
            mousePos = Transform.ScreenToWorldPos(mousePos, Camera.current);
            transform.LookAt(mousePos);
        }
    }
}
