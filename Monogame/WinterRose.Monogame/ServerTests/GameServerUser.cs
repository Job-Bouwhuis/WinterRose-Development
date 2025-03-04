using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WinterRose.Networking.TCP;

namespace WinterRose.Monogame.Servers;

public class GameServerUser : ObjectBehavior
{
    private class ObjectPosition
    {
        public ObjectPosition(WorldObject obj, bool newEntry = true)
        {
            Pos = obj.transform.position;
            NewEntry = newEntry;
        }

        public Vector2 Pos { get; set; } = new Vector2(float.NaN, float.NaN);
        public bool NewEntry { get; set; } = true;

        public static implicit operator ObjectPosition(WorldObject obj) => new(obj);
    }

    TCPUser user = new();

    Dictionary<WorldObject, ObjectPosition> objectPositions = [];

    public GameServerUser(string? ip, int port, bool makeConsole)
    {
        if (makeConsole)
        {
            Windows.OpenConsole();
        }
        user.OnMessageReceived += User_OnMessageReceived;

        user.Connect(ip ?? IPAddress.Loopback.ToString(), port);

        ExitHelper.GameClosing += GameClosing;
    }

    protected override void Awake()
    {
        world.Objects.Foreach(x => objectPositions.Add(x, x));
    }

    private void User_OnMessageReceived(string message, TCPUser self, TCPClientInfo? sender)
    {
        if (message.StartsWith("Pos "))
        {
            string[] split = message.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (split.Length == 4)
            {
                string objName = split[1];
                if (float.TryParse(split[2], out float x) && float.TryParse(split[3], out float y))
                {


                    var obj = transform.world.Objects.FirstOrDefault(x => x.Name == objName) 
                        ?? throw new Exception($"Object with name {objName} not found.");
                    var newPos = new Vector2(x, y);

                    if (obj.transform.position != newPos)
                    {
                        Console.WriteLine($"Received position update for {objName} - {x}, {y}");

                        obj.transform.position = newPos;
                        objectPositions[obj] = obj;
                    }
                }
            }
        }
    }

    protected override void Update()
    {
        world.Objects.Foreach(obj =>
        {
            if (objectPositions.TryGetValue(obj, out ObjectPosition? value))
            {
                if (obj.transform.position != value.Pos)
                    value.NewEntry = true;
                else
                    value.NewEntry = false;
            }
            else
                objectPositions[obj] = obj;

        });

        foreach (var obj in objectPositions)
        {
            if (obj.Value.NewEntry)
            {
                _ = Task.Run(() => user.Send($"Pos {obj.Key.Name} {obj.Key.transform.position.X} {obj.Key.transform.position.Y}"));
                objectPositions[obj.Key] = obj.Key;
            }
        }
    }

    protected override void Close()
    {
        Windows.CloseConsole();
    }

    private void GameClosing()
    {
        user.Disconnect();
        user.Dispose();
    }
}
