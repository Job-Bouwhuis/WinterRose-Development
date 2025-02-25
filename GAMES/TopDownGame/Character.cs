using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame.DamageSystem;
using WinterRose.Monogame.ModdingSystem;

namespace TopDownGame;

/// <summary>
/// A character in the game
/// </summary>
public class Character
{
    public string Name { get; set; }
    public long ID { get; set; }
    public ModContainer<Character> modContainer { get; } = new();
    public Vitality Vitality { get; set; }

}
