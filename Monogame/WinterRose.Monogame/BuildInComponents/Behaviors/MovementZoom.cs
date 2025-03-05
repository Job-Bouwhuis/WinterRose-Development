using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.BuildInComponents.Behaviors
{
    /// <summary>
    /// Zooms the camera based on WASD input
    /// </summary>
    public class MovementZoom : ObjectBehavior
    {
        public float MaxZoomOut { get; set; } = .7f;
        public float ZoomInSpeed { get; set; } = 600f;
        public float SpeedSimulate { get; set; } = 400;


        protected override void Update()
        {
            Vector2 inputDirection = Input.GetNormalizedWASDInput();

            if (Camera.current != null)
            {
                float targetZoom;
                if (inputDirection.LengthSquared() > 0) 
                {
                    float movementMagnitude = inputDirection.Length() * SpeedSimulate;
                    targetZoom = 1.0f - (movementMagnitude * 5f);

                    targetZoom = Math.Clamp(targetZoom, MaxZoomOut, 1f);

                    Camera.current.Zoom = MathS.Lerp(Camera.current.Zoom, targetZoom, 2 * Time.deltaTime);
                }
                else
                {
                    Camera.current.Zoom = Math.Clamp(MathS.Lerp(Camera.current.Zoom, 1f, ZoomInSpeed * Time.deltaTime), 0, 1);
                }
            }
        }
    }
}
