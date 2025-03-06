using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopDownGame.Enemies.Movement;
using WinterRose.Monogame.Weapons;

namespace TopDownGame.Enemies;

internal abstract class Enemy
{
    public Weapon Weapon { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }

    public AIMovementController MovementController { get; set; }
 }
