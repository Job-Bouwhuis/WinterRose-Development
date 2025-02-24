using Microsoft.Xna.Framework;
using System;

namespace WinterRose.Monogame.Tests;

internal class SmoothCameraFolloww : ObjectBehavior
{
    [IgnoreInTemplateCreation]
    Camera cam;

    [IncludeInTemplateCreation]
    Transform Target { get; set; }
    [IncludeInTemplateCreation]
    public float Speed { get; set; } = 10f;

    public SmoothCameraFolloww(Transform target)
    {
        Target = target;
    }

    public SmoothCameraFolloww()
    {

    }

    protected override void Awake()
    {
        if (!TryFetchComponent(out cam))
            throw new Exception("SmoothCameraFollow component needs Camera component attached to the same object");
    }

    protected override void Update()
    {
        Vector2 targetPos;
        if (Target is not null)
            targetPos = Target.position;
        else
            targetPos = new(0, 0);

        // calculate the position relative so the camera is always centered on the target
        var relativePos = new Vector2(
                           targetPos.X - (cam.transform.position.X - (cam.Bounds.X / 100000)),
                           targetPos.Y - (cam.transform.position.Y - (cam.Bounds.Y / 100000)));

        // make the camera position lerp to the target position
        transform.position += relativePos * (Speed * (float)Time.SinceLastFrame);
    }
}
