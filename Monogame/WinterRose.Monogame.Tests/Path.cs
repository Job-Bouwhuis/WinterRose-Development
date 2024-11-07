using Microsoft.Xna.Framework;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame;

namespace TowerDefenceGame.Prefabs
{
    internal class Path(string name) : Prefab(name)
    {
        public List<PathPositionData> positions;
        public List<PathPositionData> walkPoints;

        public int Width = 0;

        public override void Load()
        {
            try
            {
                positions = [];
                walkPoints = [];
                foreach (var line in File)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] parts = line.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (parts.Length == 1)
                    {
                        Width = int.Parse(parts[0]);
                        continue;
                    }

                    var start = ParseVec(parts[0]);
                    var end = ParseVec(parts[1]);
                    positions.Add(new(start, end));

                    start = ParseVec(parts[2]);
                    end = ParseVec(parts[3]);
                    walkPoints.Add(new(start, end));
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private Vector2 ParseVec(in string data)
        {
            string[] parts = data.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return new(Convert.ToSingle(parts[0]), Convert.ToSingle(parts[1]));
        }

        public override void Save()
        {
            File.WriteContent(" ", true);
            foreach (var data in positions)
            {
                File.WriteContent($"{data.Start.X};{data.Start.Y}-{data.End.X};{data.End.Y}");
            }
            File.WriteContent(Width.ToString());
        }

        public override void Unload()
        {
            positions = null;
        }

        public readonly struct PathPositionData(Vector2 start, Vector2 end)
        {
            public readonly Vector2 Start => start;
            public readonly Vector2 End => end;
        }
    }
}
