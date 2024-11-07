using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame.Worlds;
using WinterRose.Networking.TCP;

namespace WinterRose.Monogame.Servers
{
    internal class GameServerUser : ObjectBehavior
    {
        TCPUser user = new TCPUser();

        Dictionary<WorldObject, Vector2> objectPositions = []; 

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



        private void Awake()
        {
            world.Objects.Foreach(x => objectPositions.Add(x, x.transform.position));
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
                        

                        var obj = transform.world.Objects.FirstOrDefault(x => x.Name == objName);
                        if (obj is null)
                            throw new Exception($"Object with name {objName} not found.");

                        var newPos = new Vector2(x, y);

                        if(obj.transform.position != newPos)
                        {
                            Console.WriteLine($"Received position update for {objName} - {x}, {y}");

                            obj.transform.position = newPos;
                            objectPositions[obj] = obj.transform.position;
                        }
                    }
                }
            }
        }

        private void Update()
        {
            foreach (var obj in objectPositions)
            {
                if (obj.Key.transform.position != obj.Value)
                {   
                    user.Send($"Pos {obj.Key.Name} {obj.Key.transform.position.X} {obj.Key.transform.position.Y}");
                    objectPositions[obj.Key] = obj.Key.transform.position;
                }
            }
        }

        private void Close()
        {
            Windows.CloseConsole();
        }

        private void GameClosing()
        {
            user.Disconnect();
            user.Dispose();
        }
    }
}
