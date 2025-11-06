using Raylib_cs;
using WinterRose.ForgeWarden;

namespace WinterRose.FrostWarden.Tests;
internal class CharacterController : Component, IUpdatable
{
    float MoveSpeed = 500;

    public void Update()
    {
        Vector3 vec = new();
        if (Input.IsDown(KeyboardKey.W))
            vec.Y = -1;
        if (Input.IsDown(KeyboardKey.S))
            vec.Y = 1;

        if (Input.IsDown(KeyboardKey.D))
            vec.X = 1;
        if (Input.IsDown(KeyboardKey.A))
            vec.X = -1;

        if (vec != Vector3.Zero)
            vec = Vector3.Normalize(vec);

        transform.position += vec * MoveSpeed * Time.deltaTime;

        //float scaleZ = ((MathF.Sin(Time.sinceStartup) + 1f) / 2f) * 9f + 1f;
        //Camera.main.transform.scale = new Vector3(1, 1, scaleZ);
    }
}
