using WinterRose.Monogame;
using WinterRose.Monogame.Worlds;
using WinterRose;
using System;
using Microsoft.Xna.Framework;

namespace Tests;

public class TestPostBuildComponent : ObjectBehavior
{
    protected override void Update()
    {
        // if (Input.GetKeyDown(Microsoft.Xna.Framework.Input.Keys.E))
        // {
        //  World world = Universe.CurrentWorld;
        // var obj = world.CreateObject<SpriteRenderer>("spawnedObject", 20, 20, new Color(TypeWorker.CastPrimitive<byte>(new Random().Next(0, 255)), TypeWorker.CastPrimitive<byte>(new Random().Next(0, 255)), TypeWorker.CastPrimitive<byte>(new Random().Next(0, 255))));
        //obj.transform.position = transform.position;
        //}

        if (Input.GetMouse(MouseButton.Left))
        {
            World world = Universe.CurrentWorld;
            var obj = world.CreateObject<SpriteRenderer>("spawnedObject", 20, 20, 
                new Color(TypeWorker.CastPrimitive<byte>(new Random().Next(0, 255)), 
                TypeWorker.CastPrimitive<byte>(new Random().Next(0, 255)), 
                TypeWorker.CastPrimitive<byte>(new Random().Next(0, 255))));

            obj.transform.position = Transform.ScreenToWorldPos(Input.MousePosition, Camera.current);
        }
    }
}