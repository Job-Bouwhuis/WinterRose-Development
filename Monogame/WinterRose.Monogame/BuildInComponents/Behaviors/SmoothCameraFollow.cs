using Microsoft.Xna.Framework;
using System;
using WinterRose.Serialization;

namespace WinterRose.Monogame;

/// <summary>
/// Smoothly follows a target object. Requires a camera component to be attached to the same object.
/// </summary>
[RequireComponent<Camera>(AutoAdd = true)]
public class SmoothCameraFollow : ObjectBehavior
{
    [IgnoreInTemplateCreation]
    Camera cam;

    /// <summary>
    /// The transform of the target object to follow.
    /// </summary>
    [IncludeInTemplateCreation, IncludeWithSerialization]
    public Transform Target { get; set; }

    /// <summary>
    /// The speed at which the camera follows the target.
    /// </summary>
    [IncludeInTemplateCreation, IncludeWithSerialization]
    public float Speed { get; set; } = 10f;

    /// <summary>
    /// Creates a new instance of the <see cref="SmoothCameraFollow"/> class.
    /// </summary>
    /// <param name="target"></param>
    public SmoothCameraFollow(Transform target)
    {
        Target = target;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="SmoothCameraFollow"/> class.
    /// </summary>
    public SmoothCameraFollow() { }

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
        transform.position += relativePos * (Speed * (float)Time.deltaTime);
    }
}
