using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden;
public class SmoothCamera2DMode : Component, IUpdatable
{
    public Transform Target { get; set; }
    public float Speed { get; set; } = 5f;

    public void Update()
    {
        if (Target == null)
            return;

        Vector3 current = transform.position;
        Vector3 target = Target.position;

        // lerp only X/Y — keep current Z intact
        float newX = Lerp(current.X, target.X, Speed * Time.deltaTime);
        float newY = Lerp(current.Y, target.Y, Speed * Time.deltaTime);

        transform.position = new Vector3(newX, newY, current.Z);
    }

    public static float Lerp(float a, float b, float t)
    {
        if (t <= 0f) return a;
        if (t >= 1f) return b;
        return a + (b - a) * t;
    }
}
